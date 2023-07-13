using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace Core.Infrastructure.Helper;
/// <summary>
/// 解决方案解析帮助类
/// </summary>
public class SolutionHelper : IDisposable
{
    public MSBuildWorkspace Workspace { get; set; }
    public Solution Solution { get; set; }

    public SolutionHelper(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("解决方案文件不存在");
        }

        Workspace = MSBuildWorkspace.Create();
        Solution = Workspace.OpenSolutionAsync(path).Result;
    }


    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public async Task AddExistProject(string projectPath)
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
    /// <param name="newName"></param>
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
                var root = d.GetSyntaxRootAsync().Result;
                var newRoot = root.ReplaceNodes(root.DescendantNodes().OfType<NamespaceDeclarationSyntax>(), (oldNode, newNode) =>
                {
                    var node = (NamespaceDeclarationSyntax)oldNode;
                    if (node.Name.ToString() == oldName)
                    {
                        return node.WithName(SyntaxFactory.ParseName(newName));
                    }
                    return node;
                });
                var newDocument = d.WithSyntaxRoot(newRoot);
                Workspace.TryApplyChanges(newDocument.Project.Solution);
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


    public void Dispose()
    {
        Workspace.Dispose();
    }
}
