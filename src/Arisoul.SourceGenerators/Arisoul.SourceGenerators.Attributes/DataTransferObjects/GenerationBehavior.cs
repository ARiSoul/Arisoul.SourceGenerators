namespace Arisoul.SourceGenerators.DataTransferObjects;

/// <summary>
/// Defines an enum to customize extensions class generation behavior. Default is Full.
/// </summary>
public enum GenerationBehavior
{
    /// <summary>
    /// The default value. It will generate extensions for Dto and Poco classes, with methods To and For for both.
    /// </summary>
    Full = 0,
    /// <summary>
    /// Generates only extension for Dto class. dto.FromPoco(poco) and dto.ToPoco() will be generated.
    /// </summary>
    OnlyDto = 1,
    /// <summary>
    /// Generates only extension for Poco class. poco.FromDto(dto) and poco.ToDto() will be generated.
    /// </summary>
    OnlyPoco = 2,
    /// <summary>
    /// Only dto.ToPoco() and poco.ToDto() will be generated.
    /// </summary>
    OnlyToMethods = 3,
    /// <summary>
    /// Only dto.FromPoco(poco) and poco.FromDto(dto) will be generated.
    /// </summary>
    OnlyFromMethods = 4,
    /// <summary>
    /// No extensions class will be generated. There are already a lot of mappers packages out there, and you are free to use one of them.
    /// </summary>
    NoGeneration = 5
}
