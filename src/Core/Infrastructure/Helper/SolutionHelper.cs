using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 解决方案解析帮助类
/// </summary>
public class SolutionHelper : IDisposable
{
    public MSBuildWorkspace Workspace { get; set; }
    protected Solution Solution { get; private set; }
    public List<Project> Projects { get; private set; }


    public SolutionHelper(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("解决方案文件不存在");
        }
        try
        {
            MSBuildLocator.RegisterDefaults();
            Workspace = MSBuildWorkspace.Create();
            Solution = Workspace.OpenSolutionAsync(path).Result;

            Projects = Solution.Projects.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }


    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public async Task<bool> AddExistProjectAsync(string projectPath)
    {
        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException("项目文件不存在:" + projectPath);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"sln {Solution.FilePath} add {projectPath}", out string _))
        {
            return false;
        }
        Solution = await Workspace.OpenSolutionAsync(Solution.FilePath!);
        Projects = Solution.Projects.ToList();
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
        Projects = Solution.Projects.ToList();
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
        var projects = Solution.Projects;
        if (projectName != null)
        {
            projects = projects.Where(p => p.Name == projectName);
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
                    var content = File.ReadAllText(path);

                    var newNamespace = string.IsNullOrWhiteSpace(newName) ? string.Empty : "namespace " + newName;
                    var newUsing = string.IsNullOrWhiteSpace(newName) ? string.Empty : "using " + newName;
                    content = content.Replace("namespace " + oldName, newNamespace)
                                     .Replace("using " + oldName, newUsing);
                    File.WriteAllText(d.FilePath, content, Encoding.UTF8);
                }
            });
        });
    }

    /// <summary>
    /// 从解决方案中移除项目
    /// </summary>
    /// <param name="projectName"></param>
    public async Task<bool> RemoveProjectAsync(string projectName)
    {
        var project = Projects.FirstOrDefault(p => p.Name == projectName);
        if (project != null)
        {
            if (!ProcessHelper.RunCommand("dotnet", $"sln {Solution.FilePath} remove {project.FilePath}", out string _))
            {
                return false;
            }
            Solution = await Workspace.OpenSolutionAsync(Solution.FilePath!);
            Projects = Solution.Projects.ToList();
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
        Projects = Solution.Projects.ToList();
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
        var project = Solution.Projects.FirstOrDefault(p => p.Name == projectName);
        if (project == null)
        {
            await Console.Out.WriteLineAsync(" can't find project:" + projectName);
            return;
        }

        var document = project?.Documents.FirstOrDefault(d => d.FilePath == documentPath);
        if (document != null)
        {
            document = document.WithFilePath(newPath);
            var syntaxTree = await document.GetSyntaxTreeAsync();
            var unitRoot = syntaxTree!.GetCompilationUnitRoot();

            namespaceName ??= project!.Name;
            var qualifiedName = SyntaxFactory.ParseName(namespaceName);
            var usingDirective = SyntaxFactory.UsingDirective(qualifiedName);
            unitRoot = unitRoot.AddUsings(usingDirective);
            document = document.WithSyntaxRoot(unitRoot);

            // update document to solution
            Solution = Solution.WithDocumentSyntaxRoot(document.Id, document.GetSyntaxRootAsync().Result!);

            // move file
            File.Delete(documentPath);
            await File.WriteAllTextAsync(newPath, document.GetTextAsync().Result!.ToString(), Encoding.UTF8);
        }
    }

    public bool Save()
    {
        return Workspace.TryApplyChanges(Solution);
    }

    public void Dispose()
    {
        Workspace.Dispose();
    }
}
