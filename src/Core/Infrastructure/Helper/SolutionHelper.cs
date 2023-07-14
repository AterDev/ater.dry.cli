using Microsoft.CodeAnalysis.MSBuild;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 解决方案解析帮助类
/// </summary>
public class SolutionHelper : IDisposable
{
    public MSBuildWorkspace Workspace { get; set; }
    protected Solution Solution { get; set; }

    public List<Project> Projects { get; set; }

    public SolutionHelper(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("解决方案文件不存在");
        }

        try
        {
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
    public async Task AddExistProjectAsync(string projectPath)
    {
        var file = new FileInfo(projectPath);
        var project = await Workspace.OpenProjectAsync(file.FullName);

        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), project.Name, project.AssemblyName, project.Language, projectPath, project.OutputFilePath);
        Solution = Workspace.CurrentSolution.AddProject(projectInfo);
    }

    /// <summary>
    /// 添加项目引用
    /// </summary>
    /// <param name="currentProject"></param>
    /// <param name="referenceProject"></param>
    public void AddProjectRefrence(Project currentProject, Project referenceProject)
    {
        _ = Workspace.CurrentSolution.AddProjectReference(currentProject.Id, new ProjectReference(referenceProject.Id));
    }

    /// <summary>
    /// 重命名Namesapce
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
                if (d.FilePath != null)
                {
                    var content = File.ReadAllText(d.FilePath);

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
    /// <param name="projectPath"></param>
    public async void RemoveProject(string projectPath)
    {
        var file = new FileInfo(projectPath);
        var project = await Workspace.OpenProjectAsync(file.FullName);
        _ = Workspace.CurrentSolution.RemoveProject(project.Id);
    }

    /// <summary>
    /// 删除项目引用
    /// </summary>
    /// <param name="currentProject"></param>
    /// <param name="referenceProjectPath"></param>
    public void RemoveProjectReference(Project currentProject, Project referenceProjectPath)
    {
        _ = Workspace.CurrentSolution.RemoveProjectReference(currentProject.Id, new ProjectReference(referenceProjectPath.Id));
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
