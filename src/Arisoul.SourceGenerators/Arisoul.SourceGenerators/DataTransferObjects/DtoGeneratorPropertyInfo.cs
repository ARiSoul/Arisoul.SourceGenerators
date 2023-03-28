namespace Arisoul.SourceGenerators.DataTransferObjects;

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
