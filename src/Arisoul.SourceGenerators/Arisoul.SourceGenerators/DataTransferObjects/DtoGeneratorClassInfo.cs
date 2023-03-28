namespace Arisoul.SourceGenerators.DataTransferObjects;

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
