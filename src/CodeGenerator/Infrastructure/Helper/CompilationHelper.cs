using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeGenerator.Infrastructure.Helper;

public class CompilationHelper
{
    public CSharpCompilation Compilation { get; set; }

    public SemanticModel? SemanticModel { get; set; }
    public ITypeSymbol? ClassSymbol { get; set; }
    public SyntaxTree? SyntaxTree { get; set; }
    public CompilationHelper()
    {
        Compilation = CSharpCompilation.Create("tmp");
    }
    public CompilationHelper(string path, string? dllFilter = null)
    {
        Compilation = CSharpCompilation.Create("tmp");
        AddDllReferences(path, dllFilter);
    }
    public void AddDllReferences(string path, string? dllFilter = null)
    {
        var dlls = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories)
                  .Where(dll =>
                  {
                      if (!string.IsNullOrEmpty(dllFilter))
                      {
                          var fileName = Path.GetFileName(dll);
                          return fileName.ToLower().StartsWith(dllFilter.ToLower());
                      }
                      else
                      {
                          return true;
                      }
                  }).ToList();
        Compilation = Compilation.AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)));
    }

    public void AddSyntaxTree(string content)
    {
        SyntaxTree = CSharpSyntaxTree.ParseText(content);
        Compilation = Compilation.AddSyntaxTrees(SyntaxTree);
        SemanticModel = Compilation.GetSemanticModel(SyntaxTree);
        var classNode = SyntaxTree.GetCompilationUnitRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        ClassSymbol = SemanticModel.GetDeclaredSymbol(classNode);
    }

    /// <summary>
    /// 获取所有类型
    /// </summary>
    /// <returns></returns>
    public IEnumerable<INamedTypeSymbol> GetAllClasses()
    {
        var namespaces = Compilation.GlobalNamespace.GetNamespaceMembers();
        return GetNamespacesClasses(namespaces);
    }

    /// <summary>
    /// 获取所有枚举类型名称
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllEnumClasses()
    {
        return GetAllClasses()
            .Where(c => c.BaseType != null
                && c.BaseType.Name.Equals("Enum"))
            .Select(c => c.Name)
            .Distinct()
            .ToList();
    }

    public INamedTypeSymbol GetClass(string name)
    {
        return GetAllClasses().Where(cls => cls.Name == name).FirstOrDefault();
    }



    /// <summary>
    /// 获取的指定基类的所有子类
    /// </summary>
    /// <param name="namedTypes">要查找所有类集合</param>
    /// <param name="baseTypeName">基类名称</param>
    /// <returns></returns>
    public static IEnumerable<INamedTypeSymbol> GetClassNameByBaseType(IEnumerable<INamedTypeSymbol> namedTypes, string baseTypeName)
    {
        return namedTypes
            .Where(c => c.BaseType != null
                && c.BaseType.Name.Equals(baseTypeName))
            .ToList();
    }

    /// <summary>
    /// 获取命名空间下的类型
    /// </summary>
    /// <param name="namespaces"></param>
    /// <returns></returns>
    protected IEnumerable<INamedTypeSymbol> GetNamespacesClasses(IEnumerable<INamespaceSymbol> namespaces)
    {
        var classes = new List<INamedTypeSymbol>();
        classes = namespaces.SelectMany(n => n.GetTypeMembers()).ToList();
        var childNamespaces = namespaces.SelectMany(n => n.GetNamespaceMembers()).ToList();
        if (childNamespaces.Count > 0)
        {
            var childClasses = GetNamespacesClasses(childNamespaces);
            classes.AddRange(childClasses);
            return classes;
        }

        return classes;
    }



}
