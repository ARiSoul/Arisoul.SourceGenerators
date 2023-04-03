namespace Arisoul.SourceGenerators.DataTransferObjects;

internal class ExtensionsClassGenerationInfo 
    : BaseClassGenerationInfo
{
    public GenerationBehavior GenerationBehavior { get; set; }

    public ExtensionsClassGenerationInfo()
    {
        GenerationBehavior = GenerationBehavior.Full;
    }
}
