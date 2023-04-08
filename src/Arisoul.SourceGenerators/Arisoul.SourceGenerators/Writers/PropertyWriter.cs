namespace Arisoul.SourceGenerators.Writers;

internal static partial class PropertyWriter
{
    internal static string WritePublicPropertySimple(string type, string name, string modifier = "public", int tabsCount = 0) 
        => $"{GetTabs(tabsCount)}{modifier} virtual {type} {name} {{ get; set; }}";

    internal static string WritePropertyAttribution(string left, string right, int tabsCount = 0) 
        => $"{GetTabs(tabsCount)}{left} = {right};";

    internal static string GetTabs(int tabsCount)
    {
        string tabs = string.Empty;
        tabs = tabs.AppendTabs(tabsCount);
        
        return tabs;
    }
}
