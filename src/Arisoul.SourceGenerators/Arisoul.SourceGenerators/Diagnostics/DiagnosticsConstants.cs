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
    }
}
