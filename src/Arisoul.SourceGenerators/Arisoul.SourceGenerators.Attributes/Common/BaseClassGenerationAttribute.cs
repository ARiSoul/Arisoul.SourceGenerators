namespace Arisoul.SourceGenerators.Attributes.Common;

/// <summary>
/// Provides a base attribute for all class generation attributes.
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public abstract class BaseClassGenerationAttribute
    : Attribute
{
}
