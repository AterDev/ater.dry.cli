using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Droplet.CommandLine.Common;

/// <summary>
/// 类型解析帮助类
/// </summary>
public class ClassParseHelper
{
    /// <summary>
    /// 类名
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 命名空间
    /// </summary>
    public string NamespaceName { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    public string Comment { get; set; }
    /// <summary>
    /// 属性
    /// </summary>
    public List<PropertyInfo> PropertyInfos { get; }


    public ClassParseHelper(string filePath)
    {
        if (File.Exists(filePath))
        {
            PropertyInfos = GetPropertyInfos(filePath);

            var content = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(content);
            var root = tree.GetCompilationUnitRoot();
            // 获取当前类名
            var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            var trivia = classDeclarationSyntax.GetLeadingTrivia();
            Comment = trivia.ToString().TrimEnd(' ');
            var namespaceDeclarationSyntax = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            NamespaceName = namespaceDeclarationSyntax?.Name.ToString();
            Name = classDeclarationSyntax?.Identifier.ToString();
        }
        else
        {
            Console.WriteLine("dto file not exist, path:" + filePath);
        }
    }

    public ClassParseHelper() { }

    /// <summary>
    /// /// 获取该类的所有属性
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public List<PropertyInfo> GetPropertyInfos(string filePath)
    {
        var properties = new List<PropertyInfo>();
        if (!File.Exists(filePath)) return properties;

        var content = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetCompilationUnitRoot();
        var compilation = CSharpCompilation.Create("tmp", new[] { tree });
        var semanticModel = compilation.GetSemanticModel(tree);
        var specialTypes = new[] { "DateTime", "DateTimeOffset", "Guid" };
        properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .Select(prop =>
            {
                var trivia = prop.GetLeadingTrivia();
                var typeInfo = semanticModel.GetTypeInfo(prop.Type);
                var metadataName = typeInfo.Type.MetadataName.ToString();

                var type = prop.Type.ToString();
                var name = prop.Identifier.ToString();

                var attributeListSyntax = prop.AttributeLists
                    .Where(a => a.Attributes.Any(at => at.Name.ToString() != "Column"))
                    .Where(a => !a.ToString().Contains("Column"))
                    .ToList();

                var propertyInfo = new PropertyInfo(type, name)
                {
                    AttributeText = string.Join("\r\n",
                        attributeListSyntax.Select(a => a.ToString())),
                    Comments = trivia.ToString().TrimEnd(' '),
                };
                if (type.Equals("decimal"))
                {
                    propertyInfo.IsDecimal = true;
                }
                if (prop.Type.IsNotNull)
                {
                    propertyInfo.IsRequired = true;
                }
                var attributes = prop.DescendantNodes().OfType<AttributeSyntax>().ToList();
                if (attributes != null && attributes.Count > 0)
                {
                    var maxLength = attributes.Where(a => a.Name.ToString().Equals("MaxLength"))
                        .Select(a => a.ArgumentList.Arguments.FirstOrDefault())?
                        .FirstOrDefault();
                    var minLength = attributes.Where(a => a.Name.ToString().Equals("MinLength"))
                        .Select(a => a.ArgumentList.Arguments.FirstOrDefault())?
                        .FirstOrDefault();
                    if (maxLength != null)
                    {
                        propertyInfo.MaxLength = Convert.ToInt32(maxLength.ToString());
                    }
                    if (minLength != null)
                    {
                        propertyInfo.MinLength = Convert.ToInt32(minLength.ToString());
                    }
                }
                    // TODO:此判断不准确
                    if (((INamedTypeSymbol)typeInfo.Type).IsGenericType)
                {
                    propertyInfo.IsList = true;
                }
                else if (typeInfo.Type.OriginalDefinition.ToString() == metadataName && !specialTypes.Contains(metadataName))
                {
                    propertyInfo.IsReference = true;
                }
                return propertyInfo;
            }).ToList();
        // 获取当前类名
        var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

        // 继承的类名
        BaseListSyntax baseList = classDeclarationSyntax.BaseList;
        if (baseList != null)
        {
            var baseType = baseList.DescendantNodes().OfType<SimpleBaseTypeSyntax>()
                .FirstOrDefault(node => !node.ToFullString().StartsWith("I"))?.Type;
            var baseTypeInfo = semanticModel.GetTypeInfo(baseType);
            // 如果找到父类，则添加父类中的属性
            if (!string.IsNullOrEmpty(baseTypeInfo.Type.Name))
            {
                var dir = new FileInfo(filePath).Directory;
                // TODO:临时处理，寻找父类方式
                var parentPath = Path.Combine(dir.FullName, baseTypeInfo.Type.Name + ".cs");
                if (!File.Exists(parentPath))
                {
                    parentPath = Path.Combine(dir.Parent.FullName, "Common", baseTypeInfo.Type.Name + ".cs");
                }
                var parentProperties = GetPropertyInfos(parentPath);
                properties.AddRange(parentProperties);
            }
        }
        return properties.GroupBy(p => p.Name)
            .Select(s => s.FirstOrDefault())
            .ToList();
    }


    public List<PropertyInfo> GetPropertyInfos(string dllName, string className)
    {

        var help = new CompilationHelper("./", dllName);
        var cls = help.GetClass(className);
        if (cls == null) return default;

        var members = cls.GetMembers()
                         .Where(m => m.Kind == SymbolKind.Property)
                         .Select(m => m as IPropertySymbol)
                         .ToList();
        var props = members.Select(m => new
        {
            m.Type,
            ItemType = (m.Type as INamedTypeSymbol).TypeArguments.FirstOrDefault()?.Name,
            m.Name,
        }).ToList();

        var properties = new List<PropertyInfo>();

        properties = props.Select(p =>
        {
            var type = p.Type.Name;
            var propertyInfo = new PropertyInfo(type, p.Name);

            if (type.Equals("decimal"))
            {
                propertyInfo.IsDecimal = true;
            }

            if (type.Equals("Nullable"))
            {
                propertyInfo.IsRequired = false;
                propertyInfo.Type = (p.Type as INamedTypeSymbol).TypeArguments.FirstOrDefault()?.Name;
            }
            if (type.Equals("List"))
            {
                propertyInfo.IsList = true;
                if (!string.IsNullOrEmpty(p.ItemType))
                {
                    propertyInfo.Type = p.ItemType;
                }
            }
            if (p.Type.SpecialType == SpecialType.None
                && p.Type.Name != "Nullable"
                && p.Type.Name != "List")
            {
                propertyInfo.IsReference = true;
            }

                // TODO:处理父类
                return propertyInfo;
        }).ToList();
        return properties;
    }


}
public class PropertyInfo
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsList { get; set; } = false;
    /// <summary>
    /// 是否为引用类型
    /// </summary>
    public bool IsReference { get; set; } = false;
    public string AttributeText { get; set; }
    public string Comments { get; set; }
    public bool IsRequired { get; set; } = false;
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
        if (IsReference || Name.ToLower().Contains("password"))
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
