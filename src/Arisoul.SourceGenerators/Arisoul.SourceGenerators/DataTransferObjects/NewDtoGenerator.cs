using System.Collections.Immutable;

namespace Arisoul.SourceGenerators.DataTransferObjects;

[Generator]
public class NewDtoGenerator : IIncrementalGenerator
{
    internal const string FullyQualifiedMarkerName = "Arisoul.SourceGenerators.DataTransferObjects.DtoPropertyAttribute";
    internal const string DTO = "dto";

    // TODO: generate the extensions class, delete the old DtoGenerator and rename this one

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

            context.AddSource(dtoClassFileName, dtoClassCode);
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
        sb.Append(@$"#nullable enable

/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

using System;

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
}
