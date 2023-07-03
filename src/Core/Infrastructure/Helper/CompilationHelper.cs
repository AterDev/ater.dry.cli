using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Core.Infrastructure.Helper;

public class CompilationHelper
{
    public CSharpCompilation Compilation { get; set; }

    public SemanticModel? SemanticModel { get; set; }
    public ITypeSymbol? ClassSymbol { get; set; }
    public SyntaxTree? SyntaxTree { get; set; }
    public IEnumerable<INamedTypeSymbol> AllClass { get; set; }
    public CompilationUnitSyntax? SyntaxRoot { get; set; }

    public CompilationHelper(string path, string? dllFilter = null)
    {
        string suffix = DateTime.Now.ToString("HHmmss");
        Compilation = CSharpCompilation.Create("tmp" + suffix);

        AddDllReferences(path, dllFilter);
        AllClass = GetAllClasses();
    }
    public void AddDllReferences(string path, string? dllFilter = null)
    {
        List<string> dlls = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories)
                  .Where(dll =>
                  {
                      if (!string.IsNullOrEmpty(dllFilter))
                      {
                          string fileName = Path.GetFileName(dll);
                          return fileName.ToLower().StartsWith(dllFilter.ToLower());
                      }
                      else
                      {
                          return true;
                      }
                  }).ToList();

        Compilation = Compilation.AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public void AddSyntaxTree(string content)
    {
        SyntaxTree = CSharpSyntaxTree.ParseText(content);
        SyntaxRoot = SyntaxTree!.GetCompilationUnitRoot();
        Compilation = Compilation.AddSyntaxTrees(SyntaxTree);
        SemanticModel = Compilation.GetSemanticModel(SyntaxTree);

        ClassDeclarationSyntax? classNode = SyntaxTree.GetCompilationUnitRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        ClassSymbol = classNode == null ? null : SemanticModel.GetDeclaredSymbol(classNode);
    }

    /// <summary>
    /// 获取命名空间
    /// </summary>
    /// <returns></returns>
    public string? GetNamesapce()
    {
        IEnumerable<SyntaxNode>? rootNodes = SyntaxTree?.GetCompilationUnitRoot().DescendantNodes();
        NamespaceDeclarationSyntax? namespaceDeclarationSyntax = rootNodes!.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

        FileScopedNamespaceDeclarationSyntax? filescopeNamespaceDeclarationSyntax = rootNodes!.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();

        return namespaceDeclarationSyntax == null ?
            filescopeNamespaceDeclarationSyntax?.Name.ToString() : namespaceDeclarationSyntax.Name.ToString();
    }

    /// <summary>
    /// 获取所有类型
    /// </summary>
    /// <returns></returns>
    protected IEnumerable<INamedTypeSymbol> GetAllClasses()
    {
        IEnumerable<INamespaceSymbol> namespaces = Compilation.GlobalNamespace.GetNamespaceMembers();
        return GetNamespacesClasses(namespaces);
    }

    /// <summary>
    /// 获取所有枚举类型名称
    /// </summary>
    /// <returns></returns>
    public List<string> GetAllEnumClasses()
    {
        // TODO:枚举可以存储，不用每次获取 
        return AllClass.Where(c => c.BaseType != null
                && c.BaseType.Name.Equals("Enum"))
            .Select(c => c.Name)
            .Distinct()
            .ToList();
    }

    public INamedTypeSymbol? GetEnum(string name)
    {
        return AllClass.Where(c => c.Name == name)
            .Where(c => c.BaseType != null
                && c.BaseType.Name.Equals("Enum"))
            .FirstOrDefault();
    }

    public INamedTypeSymbol? GetClass(string name)
    {
        return AllClass.Where(cls => cls.Name == name).FirstOrDefault();
    }

    /// <summary>
    /// 获取一组类型的命名空间
    /// </summary>
    /// <param name="classNames"></param>
    public List<string> GetNamespaceNames(List<string> classNames)
    {
        return AllClass.Where(cls => classNames.Contains(cls.Name))
            .Select(cls => cls.ContainingNamespace.ToDisplayString())
            .Distinct()
            .ToList();
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
        List<INamedTypeSymbol> classes = new();
        classes = namespaces.SelectMany(n => n.GetTypeMembers()).ToList();
        List<INamespaceSymbol> childNamespaces = namespaces.SelectMany(n => n.GetNamespaceMembers()).ToList();
        if (childNamespaces.Count > 0)
        {
            IEnumerable<INamedTypeSymbol> childClasses = GetNamespacesClasses(childNamespaces);
            classes.AddRange(childClasses);
            return classes;
        }

        return classes;
    }


    /// <summary>
    /// 方法是否存在某个接口中
    /// </summary>
    /// <param name="methodContent"></param>
    /// <returns></returns>
    public bool MethodExist(string methodContent)
    {
        return SyntaxRoot!.DescendantNodes()
            .Where(n => n is MethodDeclarationSyntax)
            .Any(m => m.ToString().Contains(methodContent));
    }

    /// <summary>
    /// 向接口插入方法
    /// </summary>
    /// <param name="methodContent"></param>
    public void InsertInterfaceMethod(string methodContent)
    {
        if (SyntaxTree != null && SyntaxRoot != null)
        {
            var interfaceDeclaration = SyntaxRoot.DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>().Single();

            methodContent = $"    {methodContent}" + Environment.NewLine;
            if (SyntaxFactory.ParseMemberDeclaration(methodContent) is not MethodDeclarationSyntax methodNode)
            {
                return;
            }
            var newInterfaceDeclaration = interfaceDeclaration.AddMembers(methodNode);
            SyntaxRoot = SyntaxRoot.ReplaceNode(interfaceDeclaration, newInterfaceDeclaration);
        }
    }

    public void InsertClassMethod(string methodContent)
    {
        if (SyntaxTree != null && SyntaxRoot != null)
        {
            var classDeclaration = SyntaxRoot.DescendantNodes()
                .OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration != null)
            {
                methodContent = $"    {methodContent}" + Environment.NewLine;
                if (SyntaxFactory.ParseMemberDeclaration(methodContent) is not MethodDeclarationSyntax methodNode)
                {
                    return;
                }
                var newClassDeclaration = classDeclaration.AddMembers(methodNode);
                SyntaxRoot = SyntaxRoot.ReplaceNode(classDeclaration, newClassDeclaration);
            }
        }
    }
    public void ReplaceInterfaceImplement(string newImplementContent)
    {
        // replace interface first node  with new node 
        if (SyntaxTree != null && SyntaxRoot != null)
        {
            var interfaceNode = SyntaxRoot.DescendantNodes()
                .OfType<InterfaceDeclarationSyntax>().First();
            var oldBaseList = interfaceNode.DescendantNodes().OfType<BaseListSyntax>().Single();

            if (oldBaseList != null)
            {
                var typeName = SyntaxFactory.ParseTypeName(newImplementContent);
                var baseType = SyntaxFactory.SimpleBaseType(typeName);

                // add space and newline to baseType 
                var newColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken)
                  .WithTrailingTrivia(SyntaxFactory.Space);

                var newBaseList = SyntaxFactory.BaseList(
                  SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType))
                  .WithTrailingTrivia(SyntaxFactory.LineFeed)
                  .WithColonToken(newColonToken);

                var newInterfaceNode = interfaceNode.ReplaceNode(oldBaseList, newBaseList);
                SyntaxRoot = SyntaxRoot.ReplaceNode(interfaceNode, newInterfaceNode);
            }

        }
    }

    public void AddClassBaseType(string newImplementContent)
    {
        if (SyntaxTree != null && SyntaxRoot != null)
        {
            var classNode = SyntaxRoot.DescendantNodes()
                .OfType<ClassDeclarationSyntax>().First();

            var typeName = SyntaxFactory.ParseTypeName(newImplementContent);
            var baseType = SyntaxFactory.SimpleBaseType(typeName);

            // add space and newline to baseType 
            var newColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken)
              .WithTrailingTrivia(SyntaxFactory.Space);

            var newBaseList = SyntaxFactory.BaseList(
              SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType))
                  .WithTrailingTrivia(SyntaxFactory.LineFeed)
                  .WithColonToken(newColonToken);

            var newInterfaceNode = classNode.WithBaseList(newBaseList);
            SyntaxRoot = SyntaxRoot.ReplaceNode(classNode, newInterfaceNode);

        }
    }
    /// <summary>
    /// 获取所有属性类型
    /// </summary>
    /// <returns></returns>
    public List<string> GetPropertyTypes()
    {
        // get all properties from class
        var properties = SyntaxRoot!.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .ToList();

        // get  generic type  from PropertyDeclarationSyntax
        var genericTypes = properties
            .Select(p => p.Type)
            .OfType<GenericNameSyntax>()
            .Select(t => t.TypeArgumentList.Arguments.First().ToString())
            .ToList();

        return genericTypes;
    }

    /// <summary>
    /// 获取父类名称
    /// </summary>
    /// <returns></returns>
    public string? GetParentClassName()
    {
        var classNode = SyntaxTree!.GetCompilationUnitRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

        return classNode?.BaseList?.Types.FirstOrDefault()?.ToString();
    }

}
