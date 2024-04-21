namespace Definition.Infrastructure.Utils;


/// <summary>
/// format
/// </summary>
public static class TabConst
{
    private static int tabSize = 4;
    /// <summary>
    /// tab size:spaces
    /// </summary>
    public static int TabSize { get => tabSize; set => tabSize = value; }
    /// <summary>
    /// 2 spaces
    /// </summary>
    public const string Two = "  ";
    /// <summary>
    /// 4 spaces
    /// </summary>
    public const string Four = "    ";
    /// <summary>
    /// 8 spaces
    /// </summary>
    public const string Eight = "        ";
}

public static class TabExtensions
{
    public static string Indent(this string str, int tabNumber)
    {
        return str.PadLeft(tabNumber * TabConst.TabSize + str.Length);
    }
    public static string Indent(this string str)
    {
        return str.PadLeft(TabConst.TabSize + str.Length);
    }
    public static string IndentTab(this string str)
    {
        return str.PadLeft(TabConst.TabSize + str.Length, '\t');
    }
    public static string IndentTab(this string str, int tabNumber)
    {
        return str.PadLeft(tabNumber + str.Length, '\t');
    }
}
