namespace Arisoul.SourceGenerators.DataTransferObjects;

internal class DtoGeneratorClassInfo
{
    public string SourceClassName { get; set; }
    public string SourceNamespace { get; set; }
    
    public DtoClassGenerationInfo DtoClassGenerationInfo { get; set; }
    public ExtensionsClassGenerationInfo ExtensionsClassGenerationInfo { get; set; }

    public ICollection<DtoGeneratorPropertyInfo> Properties { get; set; }

    public DtoGeneratorClassInfo(
        string sourceClassName,
        string sourceNamespace,
        DtoClassGenerationInfo dtoClassGenerationInfo,
        ExtensionsClassGenerationInfo extensionsClassGenerationInfo, 
        ICollection<DtoGeneratorPropertyInfo> propertyInfos)
    {
        SourceClassName = sourceClassName;
        SourceNamespace = sourceNamespace;
        DtoClassGenerationInfo = dtoClassGenerationInfo;
        ExtensionsClassGenerationInfo = extensionsClassGenerationInfo;
        Properties = propertyInfos;
    }
}
