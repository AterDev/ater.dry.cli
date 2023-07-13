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


    public void RenameNamespace(string oldName, string newName, string? projectName = null)
    {

    }

    public void RemoveProject(string projectPath)
    {

    }

    public void RemoveProjectReference(Project currentProject, Project referenceProjectPath)
    {

    }


    public void Dispose()
    {
        Workspace.Dispose();
    }
}
