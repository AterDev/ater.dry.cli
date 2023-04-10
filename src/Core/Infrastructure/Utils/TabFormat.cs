namespace Core.Infrastructure.Utils;


/// <summary>
/// format
/// </summary>
public static class TabFormat
{
    private static int tabSize = 4;
    public static int TabSize { get => tabSize; set => tabSize = value; }
}

public static class TabExtenstions
{
    public static string Indent(this string str, int tabNumber)
    {
        return str.PadLeft(tabNumber * TabFormat.TabSize + str.Length);
    }
    public static string Indent(this string str)
    {
        return str.PadLeft(TabFormat.TabSize + str.Length);
    }
    public static string IndentTab(this string str)
    {
        return str.PadLeft(TabFormat.TabSize + str.Length, '\t');
    }
    public static string IndentTab(this string str, int tabNumber)
    {
        return str.PadLeft(tabNumber + str.Length, '\t');
    }
}
