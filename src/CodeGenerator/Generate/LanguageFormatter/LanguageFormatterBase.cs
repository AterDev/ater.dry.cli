using Share.Helper;
using Share.Models;

namespace CodeGenerator.Generate.LanguageFormatter;

/// <summary>
/// 抽象语言格式化器, 提供公共辅助方法。
/// </summary>
public abstract class LanguageFormatterBase : ILanguageFormatter
{
    public abstract string FormatType(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false);
    public abstract string GenerateModel(TypeMeta meta, string nsp = "");

    /// <summary>
    /// 复用 OpenApiHelper.FormatSchemaKey, 便于派生类调用 (包装一层, 方便未来替换/缓存)。
    /// </summary>
    public string FormatSchemaKey(string name) => OpenApiHelper.FormatSchemaKey(name);

    #region shared helpers
    protected static string StripGenericArity(string name)
    {
        var tick = name.IndexOf('`');
        return tick > 0 ? name[..tick] : name;
    }
    protected static bool IsListType(string type) => type.StartsWith("List<") || type.EndsWith("[]");
    protected static bool IsDictionaryType(string type) => type.StartsWith("Dictionary<");
    protected static string? ExtractGenericArgument(string type)
    {
        var args = ExtractGenericArguments(type);
        if (args.Count > 0) return args[0];
        if (type.EndsWith("[]")) return type[..^2];
        return null;
    }
    protected static string? ExtractDictionaryValueType(string type)
    {
        var args = ExtractGenericArguments(type);
        return args.Count >= 2 ? args[1] : null;
    }

    protected static IReadOnlyList<string> ExtractGenericArguments(string type)
    {
        if (string.IsNullOrWhiteSpace(type)) return [];

        var lt = type.IndexOf('<');
        var gt = type.LastIndexOf('>');
        if (lt <= 0 || gt <= lt)
        {
            return [];
        }

        var inner = type.Substring(lt + 1, gt - lt - 1);
        List<string> parts = [];
        int depth = 0;
        int start = 0;

        for (int i = 0; i < inner.Length; i++)
        {
            switch (inner[i])
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ',':
                    if (depth == 0)
                    {
                        parts.Add(inner[start..i].Trim());
                        start = i + 1;
                    }
                    break;
            }
        }

        if (start < inner.Length)
        {
            parts.Add(inner[start..].Trim());
        }

        return parts;
    }
    #endregion
}
