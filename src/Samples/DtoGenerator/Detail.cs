using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.NoGeneration)]
public class Detail
{
    [DtoProperty] public int ID { get; set; }
    [DtoProperty] public int HeaderID { get; set; }
}
