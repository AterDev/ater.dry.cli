using Core.Entities;

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
    /// <summary>
    /// 是否包括set方法
    /// </summary>
    public bool HasSet { get; set; } = true;
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
    /// 可空?
    /// </summary>
    public bool IsNullable { get; set; } = false;
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsDecimal { get; set; } = false;
    /// <summary>
    /// 尾缀，如#endregion
    /// </summary>
    public string? SuffixContent { get; set; }
    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
    public EntityInfo EntityInfo { get; set; } = default!;

    /// <summary>
    /// 转换成C#属性
    /// </summary>
    /// <param name="isInput">是否作为输入属性</param>
    /// <returns></returns>
    public string ToCsharpLine(bool isInput = false)
    {
        string? attributeText = AttributeText;
        // 默认值
        var defaultValue = DefaultValue;

        if (!string.IsNullOrEmpty(attributeText))
        {
            attributeText = attributeText.Trim();
            attributeText = $@"    {attributeText.Replace(Environment.NewLine, Environment.NewLine + "    ")}"
                + Environment.NewLine;
        }
        string nullableMark = IsNullable ? "?" : "";
        string requiredKeyword = IsRequired ? "required " : "";

        // 非输入的ViewModel
        if (!isInput)
        {
            requiredKeyword = "";
            if (IsRequired) { defaultValue = "default!"; }
        }
        // 可空移除默认值
        if (IsNullable) { defaultValue = string.Empty; }

        if (!string.IsNullOrWhiteSpace(defaultValue))
        {
            defaultValue = $" = {defaultValue};";
        }
        string content = @$"    public {requiredKeyword}{Type}{nullableMark} {Name} {{ get; set; }}{defaultValue}";
        if (!isInput && Name.ToLower().Contains("password"))
        {
            attributeText = attributeText?.Replace("    ", "    // ");
            content = @$"    // public {Type}{nullableMark} {Name} {{ get; set; }}{defaultValue}";
        }
        return $@"{CommentXml}{attributeText}{content}{SuffixContent}
";
    }
}