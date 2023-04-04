namespace Arisoul.SourceGenerators.DataTransferObjects;

public class DtoGeneratorPropertyInfo
{
    public string SourceName { get; set; }
    public string TargetName { get; set; }
    public string SourceType { get; }
    public string TargetType { get; }

    public DtoGeneratorPropertyInfo(string sourceName, string targetName, string sourceType, string targetType)
    {
        SourceName = sourceName;
        TargetName = targetName;
        SourceType = sourceType;
        TargetType = targetType;
    }
}
