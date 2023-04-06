namespace Arisoul.SourceGenerators.Extensions;

internal static class PropertyDeclarationSyntaxExtension
{
    public static bool IsCollection(this ITypeSymbol typeSymbol, SemanticModel semanticModel)
    {
        return typeSymbol is INamedTypeSymbol namedTypeSymbol && (
            namedTypeSymbol.Name == "ICollection" ||
            namedTypeSymbol.Name == "IEnumerable" ||
            namedTypeSymbol.Name == "IList" ||
            namedTypeSymbol.Name == "IDictionary" ||
            namedTypeSymbol.Name == "IReadOnlyCollection" ||
            namedTypeSymbol.Name == "IReadOnlyDictionary" ||
            namedTypeSymbol.Name == "IReadOnlyList" ||
            namedTypeSymbol.Name == "HashSet" ||
            namedTypeSymbol.Name == "Queue" ||
            namedTypeSymbol.Name == "SortedDictionary" ||
            namedTypeSymbol.Name == "SortedList" ||
            namedTypeSymbol.Name == "SortedSet" ||
            namedTypeSymbol.Name == "Stack" ||
            namedTypeSymbol.Name == "List" ||
            namedTypeSymbol.Name == "Dictionary" ||
            namedTypeSymbol.Name == "Enumerable" ||
            namedTypeSymbol.Name == "Collection" ||
            namedTypeSymbol.Name == "ReadOnlyCollection" ||
            namedTypeSymbol.Name == "ReadOnlyDictionary" ||
            namedTypeSymbol.Name == "ReadOnlyList" ||
            namedTypeSymbol.Name == "ConcurrentBag");
    }
}
