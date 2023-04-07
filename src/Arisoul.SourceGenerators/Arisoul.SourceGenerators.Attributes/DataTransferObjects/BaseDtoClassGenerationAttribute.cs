using Arisoul.SourceGenerators.Attributes.Common;

namespace Arisoul.SourceGenerators.DataTransferObjects;

/// <summary>
/// Defines a base class to be used by all classes generation attributes, with shared properties.
/// </summary>
/// <seealso cref="System.Attribute" />
public abstract class BaseDtoClassGenerationAttribute 
    : BaseClassGenerationAttribute
{
    /// <summary>
    /// Gets or sets the class name.
    /// </summary>
    /// <value>
    /// The class name.
    /// </value>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the class namespace.
    /// </summary>
    /// <value>
    /// The class namespace.
    /// </value>
    public string Namespace { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDtoClassGenerationAttribute"/> class.
    /// </summary>
    public BaseDtoClassGenerationAttribute()
    {
        Name = string.Empty;
        Namespace = string.Empty;
    }
}
