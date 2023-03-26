namespace Arisoul.SourceGenerators.DataTransferObjects;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DtoPropertyAttribute : Attribute
{
    public string? Name { get; set; }

    public DtoPropertyAttribute()
    {
    }

    public DtoPropertyAttribute(string name)
    {
        Name = name;
    }
}
