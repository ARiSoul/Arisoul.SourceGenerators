namespace Arisoul.SourceGenerators.DataTransferObjects;

public class DtoGeneratorPropertyInfo
{
    public string SourceName { get; set; }
    public string TargetName { get; set; }
    public string SourceType { get; }
    public string TargetType { get; }
    public bool IsChildProperty { get; set; }

    public DtoGeneratorPropertyInfo(string sourceName, string targetName, string sourceType, string targetType, bool isChildProperty)
    {
        SourceName = sourceName;
        TargetName = targetName;
        SourceType = sourceType;
        TargetType = targetType;
        IsChildProperty = isChildProperty;
    }
}
