using LiteDB;

namespace Core.Models;

public class PropertyInfo : EntityBase
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? DisplayName { get; set; }
    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsList { get; set; } = false;
    public bool IsPublic { get; set; } = true;
    /// <summary>
    /// 是否为导航属性
    /// </summary>
    public bool IsNavigation { get; set; } = false;
    /// <summary>
    /// 导航属性类名称
    /// </summary>
    public string? NavigationName { get; set; }
    /// <summary>
    /// 导航属性的对应关系
    /// </summary>
    public bool? HasMany { get; set; }
    public bool IsEnum { get; set; } = false;
    public string? AttributeText { get; set; }
    /// <summary>
    /// xml comment
    /// </summary>
    public string? CommentXml { get; set; }
    /// <summary>
    /// comment summary
    /// </summary>
    public string? CommentSummary { get; set; }
    /// <summary>
    /// 是否必须
    /// </summary>
    public bool IsRequired { get; set; } = false;
    /// <summary>
    /// 可空？
    /// </summary>
    public bool IsNullable { get; set; } = false;
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsDecimal { get; set; } = false;
    /// <summary>
    /// 尾缀，如#endregion
    /// </summary>
    public string? SuffixContent { get; set; }

    public EntityInfo EntityInfo { get; set; } = default!;
    /// <summary>
    /// 转换成C#属性
    /// </summary>
    /// <returns></returns>
    public string ToCsharpLine()
    {
        string? attributeText = AttributeText;
        if (!string.IsNullOrEmpty(attributeText))
        {
            attributeText = attributeText.Trim();
            attributeText = $@"    {attributeText.Replace(Environment.NewLine, Environment.NewLine + "    ")}"
                + Environment.NewLine;
        }
        string nullableMark = IsNullable ? "?" : "";
        string requiredKeyword = IsRequired ? "required " : "";
        string content = @$"    public {requiredKeyword}{Type}{nullableMark} {Name} {{ get; set; }}";
        if (Name.ToLower().Contains("password"))
        {
            attributeText = attributeText?.Replace("    ", "    // ");
            content = @$"    // public {Type}{nullableMark} {Name} {{ get; set; }}";
        }
        return $@"{CommentXml}{attributeText}{content}{SuffixContent}
";
    }
}