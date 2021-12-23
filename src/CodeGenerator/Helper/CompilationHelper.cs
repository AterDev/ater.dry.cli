
namespace CodeGenerator.Common;

public class CompilationHelper
{
    public string ProjectPath { get; }
    public string ProjectName { get; }

    public CSharpCompilation Compilation { get; }
    public CompilationHelper(string path, string projectName = "")
    {
        ProjectPath = path;
        ProjectName = projectName?.ToLower() ?? null;

        var dlls = Directory.EnumerateFiles(ProjectPath, "*.dll", SearchOption.AllDirectories)
                    .Where(dll =>
                    {
                        if (!string.IsNullOrEmpty(ProjectName))
                        {
                            var fileName = Path.GetFileName(dll);
                            return fileName.ToLower().StartsWith(ProjectName);
                        }
                        else
                        {
                            return true;
                        }
                    }).ToList();
        Compilation = CSharpCompilation.Create("tmp")
            .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)));
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
