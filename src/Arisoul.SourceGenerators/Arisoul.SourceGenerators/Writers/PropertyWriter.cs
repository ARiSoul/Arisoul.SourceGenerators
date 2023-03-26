namespace Arisoul.SourceGenerators.Writers;

internal static partial class Writer
{
    internal static string WritePropertySimple(string type, string name, int tabsCount = 0) 
        => $"{GetTabs(tabsCount)}public {type} {name} {{ get; set; }}";

    internal static string WritePropertyAttribution(string left, string right, int tabsCount = 0) 
        => $"{GetTabs(tabsCount)}{left} = {right};";

    internal static string GetTabs(int tabsCount)
    {
        string tabs = string.Empty;
        tabs = tabs.AppendTabs(tabsCount);
        
        return tabs;
    }
}
