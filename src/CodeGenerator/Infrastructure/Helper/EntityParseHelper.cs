using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    protected SyntaxTree? SyntaxTree { get; set; }

    public EntityParseHelper(string filePath)
    {
        AssemblyName = GetAssemblyName(filePath);
        Parse(File.ReadAllText(filePath));
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

    public void Parse(string content)
    {
        SyntaxTree = CSharpSyntaxTree.ParseText(content);
        var root = SyntaxTree.GetCompilationUnitRoot();
        // 获取当前类名
        var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        var trivia = classDeclarationSyntax.GetLeadingTrivia();
        var namespaceDeclarationSyntax = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
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
    public List<PropertyInfo>? GetPropertyInfos()
    {
        if (SyntaxTree == null) return default;
        var properties = new List<PropertyInfo>();

        var root = SyntaxTree.GetCompilationUnitRoot();
        var compilation = CSharpCompilation.Create("tmp", new[] { SyntaxTree });
        var semanticModel = compilation.GetSemanticModel(SyntaxTree);
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
        var baseList = classDeclarationSyntax.BaseList;
        if (baseList != null)
        {
            var baseType = baseList.DescendantNodes().OfType<SimpleBaseTypeSyntax>()
                .FirstOrDefault(node => !node.ToFullString().StartsWith("I"))?.Type;
            var baseTypeInfo = semanticModel.GetTypeInfo(baseType);
            // 如果找到父类，则添加父类中的属性
            if (!string.IsNullOrEmpty(baseTypeInfo.Type.Name))
            {
                //var parentProperties = GetPropertyInfos();
                //properties.AddRange(parentProperties);
            }
        }
        return properties.GroupBy(p => p.Name)
            .Select(s => s.FirstOrDefault())
            .ToList();
    }


    public List<PropertyInfo> GetPropertyInfos(string className)
    {
        var help = new CompilationHelper();
        var filePath = AssemblyHelper.FindFileInProject(ProjectFile!.FullName, className);
        help.AddDllReferences("./", AssemblyName);
        if (File.Exists(filePath))
        {
            help.AddSyntaxTree(File.ReadAllText(filePath));
        }
        var cls = help.GetClass(className);
        if (cls == null) return default;
        var members = cls.GetMembers()
                         .Where(m => m.Kind == SymbolKind.Property)
                         .Select(m => m as IPropertySymbol)
                         .ToList();

        var baseClass = cls.BaseType.Name;
        var props = members.Select(m => new
        {
            m.Type,
            ItemType = (m.Type as INamedTypeSymbol).TypeArguments.FirstOrDefault()?.Name,
            m.Name,
            Attritutes = m.GetAttributes()
        }).ToList();

        var properties = new List<PropertyInfo>();

        properties = props.Select(p =>
        {
            var type = p.Type.Name;
            var propertyInfo = new PropertyInfo(type, p.Name);
            var attributes = p.Attritutes;
            if (attributes != null)
            {

                foreach (var attr in attributes)
                {

                    if (attr.AttributeConstructor != null)
                    {
                        var constructorParams = attr.AttributeConstructor.Parameters;
                        var argumentNames = constructorParams.Select(x => x.Name).ToArray();
                        var allArguments = attr.ConstructorArguments
                            // For unnamed args, we get the name from the array we just made
                            .Select((info, index) => new KeyValuePair<string, TypedConstant>(argumentNames[index], info))
                            // Then we use name + value from the named values
                            .Union(attr.NamedArguments.Select(x => new KeyValuePair<string, TypedConstant>(x.Key, x.Value)))
                            .Distinct();
                    }


                    var argu = attr.ConstructorArguments
                        .Select(s => new { s.Value })
                        .ToList();

                    Console.WriteLine();
                }
            }

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
