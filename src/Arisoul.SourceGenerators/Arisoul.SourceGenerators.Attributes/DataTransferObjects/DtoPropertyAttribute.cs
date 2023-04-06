namespace Arisoul.SourceGenerators.DataTransferObjects;

/// <summary>
/// Provides an attribute to define the properties to be considered in the Dto generation.
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class DtoPropertyAttribute
    : Attribute
{
    /// <summary>
    /// Gets or sets the generated property name.
    /// </summary>
    /// <value>
    /// The generated property name.
    /// </value>
    public string? Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DtoPropertyAttribute"/> class.
    /// </summary>
    public DtoPropertyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DtoPropertyAttribute"/> class.
    /// </summary>
    /// <param name="name">The generated property name.</param>
    public DtoPropertyAttribute(string name)
    {
        Name = name;
    }
}

/// <summary>
/// Provides an attribute to define the properties to be considered in the Dto generation.
/// </summary>
/// <seealso cref="Attribute" />
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
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DtoChildPropertyAttribute{T}"/> class.
    /// </summary>
    /// <param name="name">The generated property name.</param>
    public DtoChildPropertyAttribute(string name)
        : base(name) 
    {
    }
}
