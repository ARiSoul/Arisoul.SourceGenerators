namespace Arisoul.SourceGenerators.DataTransferObjects;
internal abstract class BaseClassGenerationInfo
{
    public string? Name { get; set; }
    public string? Namespace { get; set; }

    public string FullyQualifiedName => string.Concat(Namespace, ".", Name);
}
