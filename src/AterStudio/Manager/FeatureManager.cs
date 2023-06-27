using Core.Infrastructure.Helper;

namespace AterStudio.Manager;
/// <summary>
/// 功能集成
/// </summary>
public class FeatureManager
{
    private readonly ProjectContext _projectContext;
    private readonly ProjectManager _projectManager;

    public string ErrorMsg { get; set; } = string.Empty;


    public FeatureManager(ProjectContext projectContext, ProjectManager projectManager)
    {
        _projectContext = projectContext;
        _projectManager = projectManager;
    }


    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<bool> CreateNewSolutionAsync(string name, string path)
    {
        // 安装模板
        if (!ProcessHelper.RunCommand("dotnet", "new list atapi.pro", out string output))
        {
            if (!ProcessHelper.RunCommand("dotnet", "new install ater.web.templates", out _))
            {
                ErrorMsg = "安装模板失败";
                return false;
            }
        }
        // 生成项目
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new atapi.pro -o {path}", out _))
        {
            ErrorMsg = "创建项目失败";
            return false;
        }

        // 添加项目
        await _projectManager.AddProjectAsync(name, path);

        return true;
    }

}
