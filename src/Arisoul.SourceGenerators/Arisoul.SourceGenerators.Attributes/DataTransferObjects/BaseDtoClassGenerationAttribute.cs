namespace Arisoul.SourceGenerators.DataTransferObjects;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public abstract class BaseDtoClassGenerationAttribute : Attribute
{
    public string Name { get; set; }
    public string Namespace { get; set; }
}
