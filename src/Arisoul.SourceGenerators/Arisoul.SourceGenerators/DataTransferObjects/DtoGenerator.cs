using System.Collections.Immutable;

namespace Arisoul.SourceGenerators.DataTransferObjects;

[Generator]
public class DtoGenerator : IIncrementalGenerator
{
    internal const string FullyQualifiedMarkerName = "Arisoul.SourceGenerators.DataTransferObjects.DtoPropertyAttribute";
    internal const string DTO = "dto";
    internal const string POCO = "poco";

    // TODO: diagnostics when a property is readonly
    // TODO: more tests scenarios, including the diagnostics
    // TODO: package in the proper way

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // do a simple filter for classes
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // combine the selected classes with the compilation
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // generate the source using the compilation and classes
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
        => syntaxNode is ClassDeclarationSyntax;

    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // do foreach instead of LINQ due performance

        var classSyntax = (ClassDeclarationSyntax)context.Node;

        // loop through all the class members
        foreach (var member in classSyntax!.Members)
        {
            // must have attributes
            if (member.AttributeLists.Count == 0)
                continue;

            // must be a property
            var propSemanticModel = context.SemanticModel.Compilation.GetSemanticModel(member.SyntaxTree);
            if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                continue;

            // loop through attributes and find the required attribute
            foreach (var attribute in propertySymbol.GetAttributes())
            {
                // is this the attribute?
                if (attribute.AttributeClass == null || !FullyQualifiedMarkerName.Contains(attribute.AttributeClass.ToString()))
                    continue;

                // attribute found in at least one property, return the class
                return classSyntax;
            }
        }

        // none of the properties in class has the required attribute
        return null;
    }

    static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

        List<DtoGeneratorClassInfo> classesToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);

        string dtoCapitalized = DTO.Capitalize()!;

        foreach (var classToGenerate in classesToGenerate)
        {
            var dtoClassName = $"{classToGenerate.ClassName}{dtoCapitalized}";
            var dtoClassFileName = $"{dtoClassName}.g.cs";
            var dtoClassCode = GetDtoClassCode(classToGenerate, dtoClassName);
            var extensionsClassName = $"{classToGenerate.ClassName}Extensions";
            var extensionsClassFileName = $"{extensionsClassName}.g.cs";
            var extensionsClassCode = GetDtoExtensionsClassCode(classToGenerate, dtoCapitalized, dtoClassName, extensionsClassName);

            context.AddSource(dtoClassFileName, dtoClassCode);
            context.AddSource(extensionsClassFileName, extensionsClassCode);
        }
    }

    private static List<DtoGeneratorClassInfo> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> distinctClasses, CancellationToken cancellationToken)
    {
        var classesToGenerate = new List<DtoGeneratorClassInfo>();

        INamedTypeSymbol? markerAttribute = compilation.GetTypeByMetadataName(FullyQualifiedMarkerName);

        if (markerAttribute == null)
            return classesToGenerate;

        // loop through all classes
        foreach (var classDeclaration in distinctClasses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var propsToGenerate = new List<DtoGeneratorPropertyInfo>();

            // loop through all class members
            foreach (var member in classDeclaration.Members)
            {
                // must have attributes
                if (!member.AttributeLists.Any())
                    continue;

                // must be a property
                var propSemanticModel = compilation.GetSemanticModel(member.SyntaxTree);
                if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                    continue;

                // loop through attributes and find the required attribute
                foreach (var attribute in propertySymbol.GetAttributes())
                {
                    // is this the attribute?
                    if (attribute.AttributeClass == null || !FullyQualifiedMarkerName.Contains(attribute.AttributeClass.ToString()))
                        continue;

                    // this is the attribute, go on
                    string? dtoPropertyName = propertySymbol.Name;

                    // Check the constructor arguments
                    if (!attribute.ConstructorArguments.IsEmpty)
                    {
                        ImmutableArray<TypedConstant> args = attribute.ConstructorArguments;

                        // make sure we don't have any errors
                        foreach (TypedConstant arg in args)
                            if (arg.Kind == TypedConstantKind.Error)
                                // have an error, so don't try and do any generation
                                break;

                        // Use the position of the argument to infer which value is set
                        switch (args.Length)
                        {
                            case 1:
                                dtoPropertyName = (string)args[0].Value!;
                                break;
                        }
                    }

                    // now check for named arguments
                    if (!attribute.NamedArguments.IsEmpty)
                        foreach (KeyValuePair<string, TypedConstant> arg in attribute.NamedArguments)
                        {
                            TypedConstant typedConstant = arg.Value;
                            if (typedConstant.Kind == TypedConstantKind.Error)
                                // have an error, so don't try and do any generation
                                break;
                            else
                            {
                                // Use the constructor argument or property name to infer which value is set
                                switch (arg.Key)
                                {
                                    case "Name":
                                        dtoPropertyName = (string)typedConstant.Value!;
                                        break;
                                }
                            }
                        }

                    var propertyType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                    var propInfo = new DtoGeneratorPropertyInfo(propertySymbol.Name, dtoPropertyName!, propertyType);

                    propsToGenerate.Add(propInfo);
                    break;
                }
            }

            if (!propsToGenerate.Any())
                continue;

            var className = classDeclaration.Identifier.ToString();

            var @namespace = classDeclaration.GetNamespace();

            classesToGenerate.Add(new DtoGeneratorClassInfo(className, @namespace, propsToGenerate));
        }

        return classesToGenerate;
    }

    private static SourceText GetDtoClassCode(DtoGeneratorClassInfo classInfo, string dtoClassName)
    {
        var sb = new StringBuilder();
        sb.Append(@$"{ClassWriter.WriteClassHeader(true)}

{ClassWriter.WriteUsing("System")}

namespace {classInfo.Namespace}
{{
    public class {dtoClassName}
    {{");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
        public {prop.Type} {prop.DtoName} {{ get; set; }}");

        sb.Append(@"
    }
}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static SourceText GetDtoExtensionsClassCode(DtoGeneratorClassInfo classInfo, string dtoCapitalized, string dtoClassName, string extensionsClassName)
    {
        string pocoCapitalized = POCO.Capitalize()!;

        var sb = new StringBuilder();

        sb.Append($@"{ClassWriter.WriteClassHeader(true)}

{ClassWriter.WriteUsing("System")}

namespace {classInfo.Namespace}
{{
    public static class {extensionsClassName}
    {{
        public static {dtoClassName} To{dtoCapitalized}(this {classInfo.ClassName} {POCO})
        {{
            {dtoClassName} {DTO} = new {dtoClassName}();
");
        
        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {DTO}.{prop.DtoName} = {POCO}.{prop.PocoName};");

        sb.Append($@"

            return {DTO};
        }}

        public static void From{dtoCapitalized}(this {classInfo.ClassName} {POCO}, {dtoClassName} {DTO})
        {{");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {POCO}.{prop.PocoName} = {DTO}.{prop.DtoName};");

        sb.Append($@"
        }}

        public static {classInfo.ClassName} To{pocoCapitalized}(this {dtoClassName} {DTO})
        {{
            {classInfo.ClassName} {POCO} = new {classInfo.ClassName}();
");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {POCO}.{prop.PocoName} = {DTO}.{prop.DtoName};");

        sb.Append(@$"

            return {POCO};
        }}

        public static void From{pocoCapitalized}(this {dtoClassName} {DTO}, {classInfo.ClassName} {POCO})
        {{");

        foreach (var prop in classInfo.Properties)
            sb.Append(@$"
            {DTO}.{prop.DtoName} = {POCO}.{prop.PocoName};");

        sb.Append(@$"
        }}
    }}
}}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }
}
