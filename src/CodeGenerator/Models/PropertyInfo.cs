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
        var attributeText = AttributeText;
        if (!string.IsNullOrEmpty(attributeText))
        {
            attributeText = attributeText.Trim();
            attributeText = $@"    {attributeText.Replace(Environment.NewLine, Environment.NewLine + "    ")}"
                + Environment.NewLine;
        }
        var nullableMark = IsNullable ? "?" : "";
        var defaultValue = IsNullable && !IsNavigation ? "" : " = default!;";
        var content = @$"    public {Type}{nullableMark} {Name} {{ get; set; }}{defaultValue}";
        if (Name.ToLower().Contains("password"))
        {
            attributeText = attributeText?.Replace("    ", "    // ");
            content = @$"    // public {Type}{nullableMark} {Name} {{ get; set; }}";
        }
        return $@"{Comments}{attributeText}{content}
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
        return input.ToFormControl();
    }
}