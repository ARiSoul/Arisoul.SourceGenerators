namespace Arisoul.SourceGenerators.Diagnostics;

public struct DiagnosticsConstants
{
    public struct DTO
    {
        public struct ReadOnlyProperty
        {
            public const string ID = "AS001";
            public const string Title = "Read only property";
            public const string Description = "The property '{0}' is readonly and cannot be used in the DTO class.";
            public const string Category = "DTO generator";
        }

        public struct AbstractClass
        {
            public const string ID = "AS002";
            public const string Title = "Abstract class";
            public const string Description = "The class '{0}' is an abstract class and cannot be used in the DTO Generation.";
            public const string Category = "DTO generator";
        }

        public struct UnsupportedExtensionsClassGenerationWithChildProperty
        {
            public const string ID = "AS003";
            public const string Title = "Unsupported feature";
            public const string Description = "Extensions class generation does not support child properties. Set DtoExtensionsClassGenerationAttribute with GenerationBehavior to NoGeneration. You still can use the DtoGenerator in the class '{0}', but the mapping should be managed in a different way, or using an existing mapper tool. Maybe this feature will be available in the future.";
            public const string Category = "DTO generator";
        }
    }
}
