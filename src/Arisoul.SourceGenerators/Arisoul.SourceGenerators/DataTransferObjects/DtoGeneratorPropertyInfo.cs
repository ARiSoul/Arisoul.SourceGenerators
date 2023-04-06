namespace Arisoul.SourceGenerators.DataTransferObjects;

public class DtoGeneratorPropertyInfo
{
    public string SourceName { get; set; }
    public string TargetName { get; set; }
    public string SourceType { get; }
    public string TargetType { get; }
    public bool IsChildProperty { get; set; }
    public List<CollectionTypeArgument> CollectionTypeArguments { get; set; }

    public DtoGeneratorPropertyInfo(
        string sourceName,
        string targetName,
        string sourceType,
        string targetType,
        bool isChildProperty,
        List<CollectionTypeArgument> collectionTypeArguments)
    {
        SourceName = sourceName;
        TargetName = targetName;
        SourceType = sourceType;
        TargetType = targetType;
        IsChildProperty = isChildProperty;
        CollectionTypeArguments = collectionTypeArguments;
    }
}

public class CollectionTypeArgument
{
    public string CollectionName { get; set; }
    public string SourceNamespace { get; set; }
    public string Name { get; set; }

    public override string ToString() => string.Concat(SourceNamespace, ".", Name);
}
