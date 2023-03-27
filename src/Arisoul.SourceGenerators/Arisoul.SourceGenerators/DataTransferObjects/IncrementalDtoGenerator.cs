using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Threading;

namespace Arisoul.SourceGenerators.DataTransferObjects;

[Generator]
public class IncrementalDtoGenerator : IIncrementalGenerator
{
    internal const string FullyQualifiedMarkerName = "GeneratorDebugConsumer.DtoPropertyAttribute";
    internal const string DTO = "dto";
    internal const string POCO = "poco";
    internal const string Extensions = "Extensions";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classesWithAttribute = context.SyntaxProvider
            .CreateSyntaxProvider(SyntaxPredicateFilter, TransformSyntax)
            .Where(x => x != null)
            .Collect();

        context.RegisterSourceOutput(classesWithAttribute, GenerateOutput);
    }

    private bool SyntaxPredicateFilter(SyntaxNode syntaxNode, CancellationToken _)
    {
        if (syntaxNode is not ClassDeclarationSyntax classSyntax
            || classSyntax.Parent is not NamespaceDeclarationSyntax parentNamespace)
            return false;

        return classSyntax.Members.Any(x => x is PropertyDeclarationSyntax);
    }

    private DtoGeneratorClassInfo TransformSyntax(GeneratorSyntaxContext syntaxContext, CancellationToken _)
    {
        var classSyntax = syntaxContext.Node as ClassDeclarationSyntax;

        var props = new List<DtoGeneratorPropertyInfo>();

        foreach (var member in classSyntax!.Members.Where(x => x is PropertyDeclarationSyntax && x.AttributeLists.Any()))
        {
            var propSemanticModel = syntaxContext.SemanticModel.Compilation.GetSemanticModel(member.SyntaxTree);
            if (propSemanticModel == null || propSemanticModel.GetDeclaredSymbol(member) is not IPropertySymbol propertySymbol)
                continue;

            var dtoPropertyAttribute = propertySymbol
                    .GetAttributes()
                    .FirstOrDefault(x => x.AttributeClass != null && FullyQualifiedMarkerName.Contains(x.AttributeClass.ToString()));

            if (dtoPropertyAttribute != null)
            {
                string? dtoPropertyName = propertySymbol.Name;

                // Check the constructor arguments
                if (!dtoPropertyAttribute.ConstructorArguments.IsEmpty)
                {
                    ImmutableArray<TypedConstant> args = dtoPropertyAttribute.ConstructorArguments;

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
                if (!dtoPropertyAttribute.NamedArguments.IsEmpty)
                    foreach (KeyValuePair<string, TypedConstant> arg in dtoPropertyAttribute.NamedArguments)
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
                                case nameof(DtoPropertyAttribute.Name):
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

        if (!props.Any())
            return null;

        var className = classSyntax.Identifier.ToString();

        var namespaceSyntax = classSyntax.Parent as NamespaceDeclarationSyntax;
        var @namespace = namespaceSyntax!.Name.ToString();

        return new DtoGeneratorClassInfo(className, @namespace, props);
    }

    private void GenerateOutput(SourceProductionContext sourceProductionContext, ImmutableArray<DtoGeneratorClassInfo> result)
    {
        string dtoCapitalized = DTO.Capitalize()!;
        string pocoCapitalized = POCO.Capitalize()!;


        foreach (var item in result)
        {
            var dtoClassName = $"{item.ClassName}{dtoCapitalized}";
            var extensionsClassName = $"{item.ClassName}{Extensions}";
            var dtoHint = $"{dtoClassName}.g.cs";
            var extensionsHint = $"{item.ClassName}{Extensions}.g.cs";

            var dtoProps = new List<string>();
            var toDToProps = new List<string>();
            var fromDtoProps = new List<string>();
            var toPocoProps = new List<string>();
            var fromPocoProps = new List<string>();
            foreach (var prop in item.Properties)
            {
                var prefixedDtoName = $"{DTO}.{prop.DtoName}";
                var prefixedPocoName = $"{POCO}.{prop.PocoName}";

                dtoProps.Add(Writer.WritePropertySimple(prop.Type, prop.DtoName, 1));
                toDToProps.Add($"{Writer.GetTabs(2)}{prefixedDtoName} = {prefixedPocoName};");
                fromDtoProps.Add($"{Writer.GetTabs(2)}{prefixedPocoName} = {prefixedDtoName};");
                toPocoProps.Add($"{Writer.GetTabs(2)}{prefixedPocoName} = {prefixedDtoName};");
                fromPocoProps.Add($"{Writer.GetTabs(2)}{prefixedDtoName} = {prefixedPocoName};");
            }

            var dtoClassCode = SourceText.From(@$"
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by a tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

#nullable enable

namespace {item.Namespace}
{{
    public class {dtoClassName}
    {{
    {string.Join("\n", dtoProps)}
    }}
}}", Encoding.UTF8);

            var extensionsClassCode = SourceText.From(@$"
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

            sourceProductionContext.AddSource(dtoHint, dtoClassCode);
            sourceProductionContext.AddSource(extensionsHint, extensionsClassCode);
        }
    }
}

public class DtoGeneratorClassInfo
{
    public string ClassName { get; set; }
    public string Namespace { get; set; }

    public ICollection<DtoGeneratorPropertyInfo> Properties { get; set; }

    public DtoGeneratorClassInfo(string className, string @namespace, ICollection<DtoGeneratorPropertyInfo> propertyInfos)
    {
        ClassName = className;
        Namespace = @namespace;
        Properties = propertyInfos;
    }
}

public class DtoGeneratorPropertyInfo
{
    public string PocoName { get; set; }
    public string DtoName { get; set; }
    public string Type { get; }

    public DtoGeneratorPropertyInfo(string pocoName, string dtoName, string type)
    {
        PocoName = pocoName;
        DtoName = dtoName;
        Type = type;
    }
}
