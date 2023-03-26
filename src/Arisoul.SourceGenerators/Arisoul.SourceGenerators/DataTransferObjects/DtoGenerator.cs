namespace Arisoul.SourceGenerators.DataTransferObjects;


public partial class DtoGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        string dtoCapitalized = DTO.Capitalize()!;
        string pocoCapitalized = POCO.Capitalize()!;
        var compilation = context.Compilation;
        var attributeSymbol = compilation.GetTypeByMetadataName(typeof(DtoPropertyAttribute).FullName);

        foreach (var syntaxTree in context.Compilation.SyntaxTrees)
        {
            var semanticModel = context.Compilation.GetSemanticModel(syntaxTree);
            var classDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var dtoProps = new List<string>();
                var toDToProps = new List<string>();
                var fromDtoProps = new List<string>();
                var toPocoProps = new List<string>();
                var fromPocoProps = new List<string>();
                var propertyDeclarations = classDeclaration.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                foreach (var propertyDeclaration in propertyDeclarations)
                {
                    if (semanticModel.GetDeclaredSymbol(propertyDeclaration) is not IPropertySymbol propertySymbol)
                        continue;

                    var dtoPropertyAttribute = propertySymbol
                        .GetAttributes()
                        .FirstOrDefault(attr => 
                            attr.AttributeClass != null 
                            && attr.AttributeClass
                                .Equals(attributeSymbol, SymbolEqualityComparer.Default));
                    
                    if (dtoPropertyAttribute != null)
                    {
                        var propertyName = propertyDeclaration.Identifier.Text;
                        var dtoPropertyName = propertyName;

                        var nameArgument = dtoPropertyAttribute.NamedArguments.FirstOrDefault(arg => arg.Key == nameof(DtoPropertyAttribute.Name));
                        if (!nameArgument.Equals(default(KeyValuePair<string, TypedConstant>)))
                            dtoPropertyName = nameArgument.Value.Value as string;

                        var propertyType = propertySymbol.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                        dtoProps.Add(Writer.WritePropertySimple(propertyType, propertyName, 1));
                        toDToProps.Add($"{Writer.GetTabs(2)}{DTO}.{dtoPropertyName} = {POCO}.{propertyName};");
                        fromDtoProps.Add($"{Writer.GetTabs(2)}{POCO}.{propertyName} = {DTO}.{dtoPropertyName};");
                        toPocoProps.Add($"{Writer.GetTabs(2)}{POCO}.{propertyName} = {DTO}.{dtoPropertyName};");
                        fromPocoProps.Add($"{Writer.GetTabs(2)}{DTO}.{dtoPropertyName} = {POCO}.{propertyName};");
                    }
                }

                if (dtoProps.Any())
                {
                    var dtoClass = $@"namespace {classDeclaration.GetNamespace()};

public class {classDeclaration.Identifier}{dtoCapitalized}
{{
{string.Join("\n", dtoProps)}
}}";

                    var extensionsClass = $@"namespace {classDeclaration.GetNamespace()};

public static class {classDeclaration.Identifier}Extensions
{{
    public static {classDeclaration.Identifier}{dtoCapitalized} To{dtoCapitalized}(this {classDeclaration.Identifier} {POCO})
    {{
        {classDeclaration.Identifier}{dtoCapitalized} {DTO} = new {classDeclaration.Identifier}{dtoCapitalized}();

{string.Join("\n", toDToProps)}

        return {DTO};
    }}

    public static void From{dtoCapitalized}(this {classDeclaration.Identifier} {POCO}, {classDeclaration.Identifier}{dtoCapitalized} {DTO})
    {{
{string.Join("\n", fromDtoProps)}
    }}

    public static {classDeclaration.Identifier} To{pocoCapitalized}(this {classDeclaration.Identifier}{dtoCapitalized} {DTO})
    {{
        {classDeclaration.Identifier} {POCO} = new {classDeclaration.Identifier}();

{string.Join("\n", toPocoProps)}

        return {POCO};
    }}

    public static void From{pocoCapitalized}(this {classDeclaration.Identifier}{dtoCapitalized} {DTO}, {classDeclaration.Identifier} {POCO})
    {{
{string.Join("\n", fromPocoProps)}
    }}
}}";

                    context.AddSource($"{classDeclaration.Identifier}{dtoCapitalized}.g.cs", SourceText.From(dtoClass, Encoding.UTF8));
                    context.AddSource($"{classDeclaration.Identifier}Extensions.g.cs", SourceText.From(extensionsClass, Encoding.UTF8));
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}