﻿using CodeGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
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
    public FileInfo ProjectFile { get; set; }
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
        var root = SyntaxTree.GetCompilationUnitRoot();
        // 获取当前类名
        var classDeclarationSyntax = RootNodes.OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var trivia = classDeclarationSyntax.GetLeadingTrivia();
        var namespaceDeclarationSyntax = RootNodes.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        NamespaceName = namespaceDeclarationSyntax?.Name.ToString();
        Name = classDeclarationSyntax?.Identifier.ToString();
        Comment = trivia.ToString().TrimEnd(' ');
        PropertyInfos = GetPropertyInfos();
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
            var filePath = AssemblyHelper.FindFileInProject(ProjectFile.FullName, parentClassName + ".cs");
            var entity = new EntityParseHelper(filePath);
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
        var specialTypes = new[] { "DateTime", "DateTimeOffset", "Guid" };
        var listTypes = new[] { "IList", "List", "ICollection", "IEnumerable" };
        var type = syntax.Type.ToString();
        var name = syntax.Identifier.ToString();
        var typeInfo = SemanticModel.GetTypeInfo(syntax.Type);
        var propertyInfo = new PropertyInfo(type, name);
        // 移除?
        if (type.EndsWith("?"))
        {
            propertyInfo.Type = type[^1..];
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

        propertyInfo.IsList = true;


        // TODO:是否为列表，是否为自定义类型
        var metadataName = typeInfo.Type.MetadataName.ToString();
        if (typeInfo.Type.OriginalDefinition.ToString() == metadataName && !specialTypes.Contains(metadataName))
        {
            propertyInfo.IsNavigation = true;
        }
        return propertyInfo;
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
            var required = GetAttributeArguments(attributes, "Required")?
                .FirstOrDefault();
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
            return theSyntax.ArgumentList?.Arguments;
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
        var baseList = classDeclarationSyntax!.BaseList.DescendantNodes()
            .OfType<SimpleBaseTypeSyntax>();
        return baseList.Where(node => !node.ToFullString().StartsWith("I"))
            .Select(s => s.ToString())
            .ToList();
    }


}
