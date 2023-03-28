using System.Collections.Immutable;

namespace Arisoul.SourceGenerators.DataTransferObjects;

//[Generator]
public class DtoGenerator : IIncrementalGenerator
{
    internal const string FullyQualifiedMarkerName = "Arisoul.SourceGenerators.DataTransferObjects.DtoPropertyAttribute";
    internal const string DTO = "dto";
    internal const string POCO = "poco";
    internal const string Extensions = "Extensions";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // register a simple class to see if it generates
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Arisoul.g.cs",
            SourceText.From("// this is a test", Encoding.UTF8)));

        //// do a simple filter for classes
        //var classesWithAttribute = context.SyntaxProvider
        //    .CreateSyntaxProvider(
        //    predicate: static (s, _) => SyntaxPredicateFilter(s), // select classes with namespaces and properties
        //    transform: static (ctx, _) => TransformSyntax(ctx)) // granulate filter by the expected attribute
        //    .Where(static x => x is not null); // filter out things that we don't care

        //// combine the selected things with the compilation
        //var compilationAndClasses = context.CompilationProvider.Combine(classesWithAttribute.Collect());

        //context.RegisterSourceOutput(compilationAndClasses, 
        //    static (spc, source) => GenerateOutput(source.Left, source.Right, spc));
    }

    static bool SyntaxPredicateFilter(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classSyntax
            || classSyntax.Parent is not NamespaceDeclarationSyntax parentNamespace)
            return false;

        return classSyntax.Members.Any(x => x is PropertyDeclarationSyntax);
    }

    static DtoGeneratorClassInfo TransformSyntax(GeneratorSyntaxContext syntaxContext)
    {
        var classSyntax = syntaxContext.Node as ClassDeclarationSyntax;

        var props = new List<DtoGeneratorPropertyInfo>();

        // loop through all the class members
        foreach (var member in classSyntax!.Members)
        {
            // must have attributes
            if (!member.AttributeLists.Any()) 
                continue;

            // must be a property
            var propSemanticModel = syntaxContext.SemanticModel.Compilation.GetSemanticModel(member.SyntaxTree);
            if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                continue;

            // loop through attributes and find the required attribute
            foreach (var attribute in propertySymbol.GetAttributes())
            {
                // is this the attribute?
                if(attribute.AttributeClass == null || !FullyQualifiedMarkerName.Contains(attribute.AttributeClass.ToString()))
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
                            return null;

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
                            return null;
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

                props.Add(propInfo);
            }
        }

        if (!props.Any()) // there are not properties to generate
            return null;

        var className = classSyntax.Identifier.ToString();

        var namespaceSyntax = classSyntax.Parent as NamespaceDeclarationSyntax;
        var @namespace = namespaceSyntax!.Name.ToString();

        return new DtoGeneratorClassInfo(className, @namespace, props);
    }

    static void GenerateOutput(Compilation compilation, ImmutableArray<DtoGeneratorClassInfo> props, SourceProductionContext context)
    {
        string dtoCapitalized = DTO.Capitalize()!;

        foreach (var item in props)
        {
            var dtoClassName = $"{item.ClassName}{dtoCapitalized}";
            var extensionsClassName = $"{item.ClassName}{Extensions}";
            var dtoClassFileName = $"{dtoClassName}.g.cs";
            var extensionsClassFileName = $"{item.ClassName}{Extensions}.g.cs";

            var dtoProps = new List<string>();
            var toDToProps = new List<string>();
            var fromDtoProps = new List<string>();
            var toPocoProps = new List<string>();
            var fromPocoProps = new List<string>();

            foreach (var prop in item.Properties)
                AddPropertiesSyntax(dtoProps, toDToProps, fromDtoProps, toPocoProps, fromPocoProps, prop);

            SourceText dtoClassCode = GetDtoClassCode(item, dtoClassName, dtoProps);
            //SourceText extensionsClassCode = GetDtoExtensionsClassCode(dtoCapitalized, item, dtoClassName, extensionsClassName, toDToProps, fromDtoProps, toPocoProps, fromPocoProps);

            context.AddSource(dtoClassFileName, dtoClassCode);
            //context.AddSource(extensionsClassFileName, extensionsClassCode);
        }
    }

    private static void AddPropertiesSyntax(List<string> dtoProps, List<string> toDToProps, List<string> fromDtoProps, List<string> toPocoProps, List<string> fromPocoProps, DtoGeneratorPropertyInfo prop)
    {
        var prefixedDtoName = $"{DTO}.{prop.DtoName}";
        var prefixedPocoName = $"{POCO}.{prop.PocoName}";

        dtoProps.Add(Writer.WritePropertySimple(prop.Type, prop.DtoName));
        toDToProps.Add($"{Writer.GetTabs(2)}{prefixedDtoName} = {prefixedPocoName};");
        fromDtoProps.Add($"{Writer.GetTabs(2)}{prefixedPocoName} = {prefixedDtoName};");
        toPocoProps.Add($"{Writer.GetTabs(2)}{prefixedPocoName} = {prefixedDtoName};");
        fromPocoProps.Add($"{Writer.GetTabs(2)}{prefixedDtoName} = {prefixedPocoName};");
    }

    private static SourceText GetDtoExtensionsClassCode(string dtoCapitalized, DtoGeneratorClassInfo item, string dtoClassName, string extensionsClassName, List<string> toDToProps, List<string> fromDtoProps, List<string> toPocoProps, List<string> fromPocoProps)
    {
        string pocoCapitalized = POCO.Capitalize()!;

        return SourceText.From(@$"
            /*  <auto-generated> ------------------------------------------------------------------------------ 
                    This code was generated by a tool.
                    Changes to this file may cause incorrect behavior and will be lost if
                    the code is regenerated.
                </auto-generated> ------------------------------------------------------------------------------*/

            #nullable enable

            namespace {item.Namespace}
            {{
                public static class {extensionsClassName}
                {{
                    public static {dtoClassName} To{dtoCapitalized}(this {item.ClassName} {POCO})
                    {{
                        {dtoClassName} {DTO} = new {dtoClassName}();

                {string.Join("\n", toDToProps)}

                        return {DTO};
                    }}

                    public static void From{dtoCapitalized}(this {item.ClassName} {POCO}, {dtoClassName} {DTO})
                    {{
                {string.Join("\n", fromDtoProps)}
                    }}

                    public static {item.ClassName} To{pocoCapitalized}(this {dtoClassName} {DTO})
                    {{
                        {item.ClassName} {POCO} = new {item.ClassName}();

                {string.Join("\n", toPocoProps)}

                        return {POCO};
                    }}

                    public static void From{pocoCapitalized}(this {dtoClassName} {DTO}, {item.ClassName} {POCO})
                    {{
                {string.Join("\n", fromPocoProps)}
                    }}
                }}
            }}", Encoding.UTF8);
    }

    private static SourceText GetDtoClassCode(DtoGeneratorClassInfo item, string dtoClassName, List<string> dtoProps)
    {
        var sb = new StringBuilder();
        sb.Append(@$"using System;

namespace {item.Namespace}
{{
    public class {dtoClassName}
    {{");

        foreach (var prop in dtoProps)
            sb.Append(@$"
        {prop}");

        sb.Append(@"
    }
}");

        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }
}