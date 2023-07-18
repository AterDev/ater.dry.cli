using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 解决方案解析帮助类
/// </summary>
public class SolutionHelper : IDisposable
{
    public MSBuildWorkspace Workspace { get; set; }
    protected Solution Solution { get; private set; }

    public SolutionHelper(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("解决方案文件不存在");
        }
        try
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
            Workspace = MSBuildWorkspace.Create();
            Solution = Workspace.OpenSolutionAsync(path).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectName"></param>
    /// <returns></returns>
    public Project? GetProject(string projectName)
    {
        return Solution.Projects.FirstOrDefault(p => p.AssemblyName == projectName);
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public bool AddExistProject(string projectPath)
    {
        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException("项目文件不存在:" + projectPath);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"sln {Solution.FilePath} add {projectPath}", out string _))
        {
            return false;
        }
        if (Solution.Projects.Any(p => p.FilePath!.Equals(projectPath)))
        {
            return false;
        }
        var project = Workspace.OpenProjectAsync(projectPath).Result;
        // add opened project to solution
        Solution = project.Solution;
        return true;
    }

    /// <summary>
    /// 添加项目引用
    /// </summary>
    /// <param name="currentProject"></param>
    /// <param name="referenceProject"></param>
    public bool AddProjectReference(Project currentProject, Project referenceProject)
    {
        if (!ProcessHelper.RunCommand("dotnet", $"add {currentProject.FilePath} reference {referenceProject.FilePath}", out string _))
        {
            return false;
        }
        Solution = Solution.AddProjectReference(currentProject.Id, new ProjectReference(referenceProject.Id));
        return true;
    }

    /// <summary>
    /// 重命名Namespace
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName">为空时，则删除原名称</param>
    /// <param name="projectName"></param>
    public void RenameNamespace(string oldName, string newName, string? projectName = null)
    {
        var projects = Solution.Projects.GroupBy(p => p.AssemblyName)
            .Select(g => g.First());
        if (projectName != null)
        {
            projects = projects.Where(p => p.AssemblyName == projectName);
        }
        Parallel.ForEach(projects, p =>
        {
            Parallel.ForEach(p.Documents, d =>
            {
                if (d.Folders.Count > 0 && d.Folders[0].Equals("obj"))
                {
                    return;
                }
                if (d.FilePath != null)
                {
                    var path = d.FilePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                    if (!File.Exists(path))
                    {
                        return;
                    }
                    var content = File.ReadAllText(path);

                    var newNamespace = string.IsNullOrWhiteSpace(newName) ? string.Empty : "namespace " + newName;
                    var newUsing = string.IsNullOrWhiteSpace(newName) ? string.Empty : "using " + newName;
                    content = content.Replace("namespace " + oldName, newNamespace)
                                     .Replace("using " + oldName, newUsing);
                    File.WriteAllText(d.FilePath, content, new UTF8Encoding(false));
                }
            });
        });
    }

    public void RemoveAttributes(string projectName, string attributeName)
    {
        var project = Solution.Projects.FirstOrDefault(p => p.AssemblyName == projectName);
        if (project != null)
        {
            DocumentEditor editor;
            var documents = project.Documents.Where(d => d.FilePath != null && d.FilePath.EndsWith(".cs")).ToList();

            documents.ForEach(document =>
            {
                editor = DocumentEditor.CreateAsync(document).Result;
                var nodes = editor.OriginalRoot
                    .DescendantNodes().OfType<AttributeSyntax>()
                    .Where(a => a.Name.ToString() == attributeName);

                foreach (var node in nodes)
                {
                    ReplaceNodeUsing(editor, node, _ => SyntaxFactory.ParseExpression(""));
                }
                var newContent = FormatChanges(editor.GetChangedRoot());

                File.WriteAllText(document.FilePath!, newContent, new UTF8Encoding(false));
            });
        }
    }

    private string FormatChanges(SyntaxNode node)
    {
        var workspace = new AdhocWorkspace();
        var options = workspace.Options
            // change these values to fit your environment / preferences 
            .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, value: true)
            .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, value: "\r\n");
        return Formatter.Format(node, workspace, options).ToFullString();
    }

    /// <summary>
    /// 内容节点编辑
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    /// <param name="editor"></param>
    /// <param name="node"></param>
    /// <param name="replacementNode"></param>
    public void ReplaceNodeUsing<TNode>(DocumentEditor editor, TNode node, Func<TNode, SyntaxNode> replacementNode) where TNode : SyntaxNode
    {
        editor.ReplaceNode(node, (n, _) => replacementNode((TNode)n));
    }

    /// <summary>
    /// 从解决方案中移除项目
    /// </summary>
    /// <param name="projectName"></param>
    public bool RemoveProject(string projectName)
    {
        var project = Solution.Projects.FirstOrDefault(p => p.AssemblyName == projectName);
        if (project != null)
        {
            if (!ProcessHelper.RunCommand("dotnet", $"sln {Solution.FilePath} remove {project.FilePath}", out string _))
            {
                return false;
            }
            Solution = Solution.RemoveProject(project.Id);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 删除项目引用
    /// </summary>
    /// <param name="currentProject"></param>
    /// <param name="referenceProject"></param>
    public bool RemoveProjectReference(Project currentProject, Project referenceProject)
    {
        if (!ProcessHelper.RunCommand("dotnet", $"remove {currentProject.FilePath} reference {referenceProject.FilePath}", out string _))
        {
            return false;
        }

        Solution = Solution.RemoveProjectReference(currentProject.Id, new ProjectReference(referenceProject.Id));
        return true;
    }


    /// <summary>
    /// 移动文件
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="documentPath"></param>
    /// <param name="newPath"></param>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public async Task MoveDocumentAsync(string projectName, string documentPath, string newPath, string? namespaceName = null)
    {
        var project = Solution.Projects.FirstOrDefault(p => p.AssemblyName == projectName);
        if (project == null)
        {
            await Console.Out.WriteLineAsync(" can't find project:" + projectName);
            return;
        }

        var document = project?.Documents.FirstOrDefault(d => d.FilePath == documentPath);
        if (document != null)
        {
            namespaceName ??= project!.Name;
            var unitRoot = await document.GetSyntaxRootAsync();

            var namespaceSyntax = unitRoot!.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();

            if (namespaceSyntax != null)
            {
                var newNamespaceSyntax = namespaceSyntax.WithName(SyntaxFactory.ParseName(namespaceName));
                unitRoot = unitRoot.ReplaceNode(namespaceSyntax, newNamespaceSyntax);
            }

            document = document.WithSyntaxRoot(unitRoot);
            document = document.WithFilePath(newPath);

            // update document to solution
            Solution = Solution.WithDocumentSyntaxRoot(document.Id, document.GetSyntaxRootAsync().Result!);

            // move file
            File.Delete(documentPath);
            await File.WriteAllTextAsync(newPath, document.GetTextAsync().Result!.ToString(), new UTF8Encoding(false));
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="projectName"></param>
    /// <param name="documentPaths"></param>
    /// <returns></returns>
    public async Task RemoveFileAsync(string projectName, string[] documentPaths)
    {
        if (documentPaths.Length == 0)
        {
            return;
        }
        var project = Solution.Projects.FirstOrDefault(p => p.AssemblyName == projectName);
        if (project == null)
        {
            await Console.Out.WriteLineAsync(" can't find project:" + projectName);
            return;
        }
        foreach (var documentPath in documentPaths)
        {
            var document = project?.Documents.FirstOrDefault(d => d.FilePath == documentPath);
            if (document != null)
            {
                project = project!.RemoveDocument(document.Id);
                Solution = project.Solution;
                File.Delete(documentPath);
            }
        }
    }


    public bool Save()
    {
        return Workspace.TryApplyChanges(Solution);
    }

    public void Dispose()
    {
        Workspace.Dispose();
        MSBuildLocator.Unregister();
    }

}
