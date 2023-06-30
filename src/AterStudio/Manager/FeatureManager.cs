using AterStudio.Models;
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
    /// <returns></returns>
    public async Task<bool> CreateNewSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        var path = Path.Combine(dto.Path, dto.Name);
        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(dto.Path);
        }
        if (!ProcessHelper.RunCommand("dotnet", $"new atapi.pro -o {path}", out _))
        {
            ErrorMsg = "创建项目失败";
            return false;
        }
        await Console.Out.WriteLineAsync($"✅ create new solution {path}");

        // 添加项目
        //await _projectManager.AddProjectAsync(dto.Name, dto.Path);

        return true;
    }
}
