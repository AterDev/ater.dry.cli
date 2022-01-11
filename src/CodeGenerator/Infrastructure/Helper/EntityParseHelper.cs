using CodeGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using System.Text.RegularExpressions;
using static Humanizer.In;
using PropertyInfo = CodeGenerator.Models.PropertyInfo;

namespace CodeGenerator.Infrastructure.Helper;

/// <summary>
/// 类型解析帮助类
/// </summary>
public class EntityParseHelper
{
    /// <summary>
    /// 类名
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// 命名空间
    /// </summary>
    public string? NamespaceName { get; set; }
    /// <summary>
    /// 程序集名称
    /// </summary>
    public string AssemblyName { get; set; }
    public FileInfo? ProjectFile { get; set; }
    /// <summary>
    /// 类注释
    /// </summary>
    public string? Comment { get; set; }
    /// <summary>
    /// 属性
    /// </summary>
    public List<PropertyInfo>? PropertyInfos { get; set; }
    public CSharpCompilation Compilation { get; set; }
    public SemanticModel? SemanticModel { get; set; }
    protected SyntaxTree SyntaxTree { get; set; }
    public IEnumerable<SyntaxNode> RootNodes { get; set; }
    public EntityKeyType KeyType { get; set; } = EntityKeyType.Guid;
    public string[] SpecialTypes = new[] { "DateTime", "DateTimeOffset", "DateOnly", "TimeOnly", "Guid" };

    public EntityParseHelper(string filePath)
    {
        AssemblyName = GetAssemblyName(filePath);
        if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);
        var content = File.ReadAllText(filePath, Encoding.UTF8);
        SyntaxTree = CSharpSyntaxTree.ParseText(content);
        Compilation = CSharpCompilation.Create("tmp", new[] { SyntaxTree });
        SemanticModel = Compilation.GetSemanticModel(SyntaxTree);
        RootNodes = SyntaxTree.GetCompilationUnitRoot().DescendantNodes();
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    public string GetAssemblyName(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var projectFile = AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root);
        if (projectFile == null) throw new ArgumentException("can't find project file");
        ProjectFile = projectFile;
        return AssemblyHelper.GetAssemblyName(ProjectFile);
    }

    public void Parse()
    {
        // 获取当前类名
        var classDeclarationSyntax = RootNodes.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var trivia = classDeclarationSyntax!.GetLeadingTrivia();
        var namespaceDeclarationSyntax = RootNodes.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        NamespaceName = namespaceDeclarationSyntax?.Name.ToString();
        Name = classDeclarationSyntax?.Identifier.ToString();
        Comment = trivia.ToString().TrimEnd();
        PropertyInfos = GetPropertyInfos();
    }
    public EntityInfo GetEntity()
    {
        var classDeclarationSyntax = RootNodes.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var trivia = classDeclarationSyntax!.GetLeadingTrivia();
        var name = classDeclarationSyntax!.Identifier.ToString();
        var comment = trivia.ToString().TrimEnd();
        var classSymbol = SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        var namespaceName = classSymbol?.ContainingNamespace.ToString();

        return new EntityInfo(name)
        {
            AssemblyName = AssemblyName,
            NamespaceName = namespaceName,
            Comment = comment,
            PropertyInfos = GetPropertyInfos(),
            KeyType = KeyType
        };
    }

    /// <summary>
    /// 获取该类的所有属性
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public List<PropertyInfo>? GetPropertyInfos(string? parentClassName = null)
    {
        var properties = new List<PropertyInfo>();
        var root = SyntaxTree.GetCompilationUnitRoot();
        var propertySyntax = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        // 如果指定父类名称
        if (parentClassName != null)
        {
            var filePath = AssemblyHelper.FindFileInProject(ProjectFile!.FullName, parentClassName + ".cs");
            var entity = new EntityParseHelper(filePath!);
            return entity.GetPropertyInfos();
        }
        foreach (var prop in propertySyntax)
        {
            // type and name
            var propertyInfo = ParsePropertyType(prop);
            // attribute and comments text
            propertyInfo.AttributeText = GetAttributeText(prop);
            propertyInfo.Comments = GetComment(prop);
            // attributes
            ParsePropertyAttributes(prop, propertyInfo);

            properties.Add(propertyInfo);
        }
        var parentClass = GetParentClassName();
        if (parentClass != null)
        {
            var parentProperties = GetPropertyInfos(parentClass.FirstOrDefault());
            if (parentProperties != null)
                properties.AddRange(parentProperties);
        }

        return properties.GroupBy(p => p.Name)
             .Select(s => s.FirstOrDefault()!)
             .ToList();
    }

    /// <summary>
    /// 获取属性注释内容
    /// </summary>
    /// <returns></returns>
    protected string GetComment(PropertyDeclarationSyntax syntax)
    {
        var trivia = syntax.GetLeadingTrivia();
        return trivia.ToString().TrimEnd(' ');
    }
    /// <summary>
    /// 获取属性特性文本内容
    /// </summary>
    /// <returns></returns>
    protected string GetAttributeText(PropertyDeclarationSyntax syntax)
    {
        var attributeListSyntax = syntax.AttributeLists
             .Where(a => a.Attributes.Any(attr => attr.Name.ToString() != "Column"))
             .Where(a => !a.ToString().Contains("Column"))
             .ToList();
        return string.Join("\r\n", attributeListSyntax.Select(a => a.ToString()));

    }

    /// <summary>
    /// 对属性的类型进行解析
    /// </summary>
    /// <param name="syntax"></param>
    /// <param name="propertyInfo"></param>
    protected PropertyInfo ParsePropertyType(PropertyDeclarationSyntax syntax)
    {
        var listTypes = new[] { "IList", "List", "ICollection", "IEnumerable" };
        var type = syntax.Type.ToString();
        var name = syntax.Identifier.ToString();
        var typeInfo = SemanticModel.GetTypeInfo(syntax.Type);
        var propertyInfo = new PropertyInfo(type, name);

        // 是否为public
        var modifier = syntax.Modifiers.FirstOrDefault().Text;
        if (string.IsNullOrEmpty(modifier) || !modifier.ToLower().Equals("public"))
        {
            propertyInfo.IsPublic = false;
        }
        // 移除?，获取类型名
        if (type.EndsWith("?"))
        {
            propertyInfo.Type = type[..^1];
            propertyInfo.IsNullable = true;
        }

        if (typeInfo.Type!.Name.Equals("Nullable"))
        {
            propertyInfo.IsNullable = true;
        }
        if (propertyInfo.Type.StartsWith("decimal"))
        {
            propertyInfo.IsDecimal = true;
        }
        if (typeInfo.Type.TypeKind == TypeKind.Enum)
        {
            propertyInfo.IsEnum = true;
        }
        // 列表的判断
        if (propertyInfo.Type.EndsWith("[]"))
        {
            propertyInfo.IsList = true;
        }
        // 正则匹配 \s+(\w+)<(\w+)>
        string pattern = @"(?<Type>\w+)<(?<GenericType>\w+)>";
        var match = Regex.Match(type, pattern);
        if (match.Success)
        {
            if (listTypes.Contains(match.Groups["Type"]?.Value))
            {
                propertyInfo.IsList = true;
            }
        }
        // 导航属性判断
        ParseNavigation(typeInfo.Type! as INamedTypeSymbol, propertyInfo);
        return propertyInfo;
    }

    /// <summary>
    /// 解析导航属性
    /// </summary>
    /// <param name="type"></param>
    /// <param name="propertyInfo"></param>
    protected void ParseNavigation(INamedTypeSymbol type, PropertyInfo propertyInfo)
    {
        var navigationType = type;
        propertyInfo.HasMany = false;
        // 可空的列表，取泛型类型
        if (propertyInfo.IsNullable)
        {
            navigationType = navigationType.TypeArguments[0] as INamedTypeSymbol;
            if (propertyInfo.IsList)
            {
                navigationType = navigationType.TypeArguments[0] as INamedTypeSymbol;
                propertyInfo.HasMany = true;
            }
        }
        else if (propertyInfo.IsList)
        {
            navigationType = navigationType.TypeArguments[0] as INamedTypeSymbol;
            propertyInfo.HasMany = true;
        }
        if (navigationType.SpecialType == SpecialType.None
            && !SpecialTypes.Contains(navigationType.Name)
            && !propertyInfo.IsEnum)
        {
            propertyInfo.NavigationName = navigationType.Name;
            propertyInfo.IsNavigation = true;
        }
    }
    /// <summary>
    /// 解析属性特性
    /// </summary>
    /// <param name="property"></param>
    protected void ParsePropertyAttributes(PropertyDeclarationSyntax syntax, PropertyInfo propertyInfo)
    {
        var attributes = syntax.DescendantNodes().OfType<AttributeSyntax>().ToList();
        if (attributes != null && attributes.Count > 0)
        {
            var maxLength = GetAttributeArguments(attributes, "MaxLength")?
                .FirstOrDefault();
            var minLength = GetAttributeArguments(attributes, "MinLength")?
                .FirstOrDefault();
            var stringLength = GetAttributeArguments(attributes, "StringLength")?
                .FirstOrDefault();
            var required = GetAttributeArguments(attributes, "Required");
            var key = GetAttributeArguments(attributes, "Key");
            if (key != null)
            {
                KeyType = propertyInfo.Type.ToLower() switch
                {
                    "string" => EntityKeyType.String,
                    "int" => EntityKeyType.Int,
                    _ => EntityKeyType.Guid,
                };
            }

            if (required != null) propertyInfo.IsRequired = true;
            if (maxLength != null)
            {
                propertyInfo.MaxLength = Convert.ToInt32(maxLength.ToString());
            }
            if (minLength != null)
            {
                propertyInfo.MinLength = Convert.ToInt32(minLength.ToString());
            }
            if (stringLength != null)
            {
                propertyInfo.MaxLength = Convert.ToInt32(stringLength.ToString());
            }
        }
    }
    /// <summary>
    /// 获取特性中的参数内容
    /// </summary>
    /// <param name="syntax"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    protected IEnumerable<AttributeArgumentSyntax>? GetAttributeArguments(List<AttributeSyntax> syntax, string attributeName)
    {
        var theSyntax = syntax.Where(s => s.Name.ToString().ToLower().Equals(attributeName.ToLower()))
            .FirstOrDefault();
        if (theSyntax != null)
        {
            return theSyntax.ArgumentList?.Arguments ??
                new SeparatedSyntaxList<AttributeArgumentSyntax>();
        }
        return default;
    }
    /// <summary>
    /// 获取父类名称
    /// </summary>
    /// <returns></returns>
    protected List<string>? GetParentClassName()
    {
        // 获取当前类名
        var classDeclarationSyntax = RootNodes.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        // 继承的类名
        var baseList = classDeclarationSyntax!.BaseList?.DescendantNodes()
            .OfType<SimpleBaseTypeSyntax>();
        if (baseList == null) return default;
        return baseList.Where(node => !node.ToFullString().StartsWith("I"))
            .Select(s => s.ToString())
            .ToList();
    }

}
