namespace Arisoul.SourceGenerators.DataTransferObjects;

/// <summary>
/// Provides an attribute to customize the generated extensions class.
/// </summary>
public class DtoExtensionsClassGenerationAttribute
    : BaseDtoClassGenerationAttribute
{
    public GenerationBehavior GenerationBehavior { get; set; }

    public DtoExtensionsClassGenerationAttribute()
    {
        GenerationBehavior = GenerationBehavior.Full;
    }
}