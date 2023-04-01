namespace Arisoul.SourceGenerators.DataTransferObjects;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public abstract class BaseDtoClassGenerationAttribute : Attribute
{
    public string? Name { get; set; }

    public BaseDtoClassGenerationAttribute()
    {
    }

    public BaseDtoClassGenerationAttribute(string name)
    {
        Name = name;
    }
}
