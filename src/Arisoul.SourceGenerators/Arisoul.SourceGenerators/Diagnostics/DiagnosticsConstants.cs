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
    }
}
