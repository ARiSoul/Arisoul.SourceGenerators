using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.NoGeneration)]
public class Header
{
    [DtoProperty]
    public int ID { get; set; }

    [DtoChildProperty<HashSet<DetailDto>>]
    public ICollection<Detail> Details { get; set; }
}
