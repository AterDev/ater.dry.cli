﻿
namespace CodeGenerator.Models;

public class PropertyInfo
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string? DisplayName { get; set; }
    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsList { get; set; } = false;
    /// <summary>
    /// 是否为导航属性
    /// </summary>
    public bool IsNavigation { get; set; } = false;
    /// <summary>
    /// 导航属性名称
    /// </summary>
    public string? NavigationName { get; set; }
    public string? AttributeText { get; set; }
    public string? Comments { get; set; }
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
    public PropertyInfo(string type, string name)
    {
        Type = type;
        Name = name;
    }

    /// <summary>
    /// 转换成C#属性
    /// </summary>
    /// <returns></returns>
    public string ToCsharpLine()
    {
        if (!string.IsNullOrEmpty(AttributeText))
        {
            AttributeText = AttributeText.Trim();
            AttributeText = $@"        {AttributeText}
";
        }
        else
        {
            AttributeText = string.Empty;
        }
        var content = @$"        public {Type} {Name} {{ get; set; }}";
        // 如果是引用对象
        if (IsNavigation || Name.ToLower().Contains("password"))
        {
            content = @$"        // public {Type}Dto {Name} {{ get; set; }}";
        }
        return $@"{Comments}{AttributeText}{content}
";
    }

    /// <summary>
    /// 转换成前端表单控件
    /// </summary>
    /// <returns></returns>
    public string ToNgInputControl()
    {
        var input = new NgInputBuilder(Type, Name, DisplayName)
        {
            IsDecimal = IsDecimal,
            IsRequired = IsRequired,
            MaxLength = MaxLength,
            MinLength = MinLength
        };
        return input.ToString();
    }
}