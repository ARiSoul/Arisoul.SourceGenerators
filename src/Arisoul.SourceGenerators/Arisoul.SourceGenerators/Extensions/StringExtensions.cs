namespace Arisoul.SourceGenerators.Extensions;

internal static class StringExtensions
{
    internal static string AppendTabs(this string str, int tabsCount)
    {
        if (tabsCount > 0)
            for (int i = 0; i < tabsCount; i++)
                str = string.Concat(str, "\t");

        return str;
    }

    internal static string? Capitalize(this string? str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }
}