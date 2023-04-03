namespace Arisoul.SourceGenerators.Extensions;

internal static class ObjectExtensions
{
    public static bool In<T>(this T o, params T[] args) => args.Contains(o);
}
