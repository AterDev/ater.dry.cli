namespace CodeGenerator.Infrastructure.Helper;

public class CompilationHelper
{
    public CSharpCompilation Compilation { get; set; }

    public SemanticModel SemanticModel { get; set; }
    public CompilationHelper()
    {
        Compilation = CSharpCompilation.Create("tmp");
    }
    public void AddDllReferences(string path, string? dllFilter)
    {
        var dlls = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories)
                  .Where(dll =>
                  {
                      if (!string.IsNullOrEmpty(dllFilter))
                      {
                          var fileName = Path.GetFileName(dll);
                          return fileName.ToLower().StartsWith(dllFilter);
                      }
                      else
                      {
                          return true;
                      }
                  }).ToList();
        Compilation.AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)));
    }

    public void AddSyntaxTree(string content)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(content);
        Compilation.AddSyntaxTrees(syntaxTree);
        SemanticModel = Compilation.GetSemanticModel(syntaxTree);
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
    public IEnumerable<INamedTypeSymbol> GetClassNameByBaseType(IEnumerable<INamedTypeSymbol> namedTypes, string baseTypeName)
    {
        if (namedTypes == null || namedTypes.Count() < 1)
        {
            return default;
        }
        return namedTypes.Where(c => c.BaseType.Name.Equals(baseTypeName)).ToList();
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
