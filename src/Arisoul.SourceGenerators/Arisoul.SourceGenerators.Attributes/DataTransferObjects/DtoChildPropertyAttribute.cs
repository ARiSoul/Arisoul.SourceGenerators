namespace Arisoul.SourceGenerators.DataTransferObjects;

/// <summary>
/// Provides an attribute to define the not primitive child properties for DTO generation.
/// </summary>
/// <seealso cref="Attribute" />
/// <remarks>When applied, ExtensionsClass generation behavior should be NoGeneration.</remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DtoChildPropertyAttribute<T>
    : DtoPropertyAttribute
    where T : class, new()
{

    public T? TargetType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DtoChildPropertyAttribute{T}"/> class.
    /// </summary>
    public DtoChildPropertyAttribute()
        : base()
    {
        TargetType = new T();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DtoChildPropertyAttribute{T}"/> class.
    /// </summary>
    /// <param name="name">The generated property name.</param>
    public DtoChildPropertyAttribute(string name)
        : base(name) 
    {
        TargetType = new T();
    }
}
