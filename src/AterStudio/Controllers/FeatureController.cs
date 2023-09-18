using AterStudio.Advance;
using AterStudio.Manager;
using AterStudio.Models;
using Command.Share;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 功能模块
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeatureController : ControllerBase
{
    private readonly ProjectManager _manager;
    private readonly AdvanceManager _advace;
    private readonly FeatureManager _feature;
    private readonly ProjectContext _projectContext;
    public FeatureController(ProjectManager manager, AdvanceManager advace, FeatureManager feature, ProjectContext projectContext)
    {
        _manager = manager;
        _advace = advace;
        _feature = feature;
        _projectContext = projectContext;
    }

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    [HttpPost("newSolution")]
    public async Task<ActionResult<bool>> CreateNewSolution(CreateSolutionDto dto)
    {
        var res = await _feature.CreateNewSolutionAsync(dto);
        return res ? true : Problem(_feature.ErrorMsg);
    }

    /// <summary>
    /// 获取模块列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("modules")]
    public List<SubProjectInfo> GetModulesInfo()
    {
        return _feature.GetModulesInfo();
    }

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost("createModule")]
    public async Task<ActionResult<bool>> CreateModule(string name)
    {
        var res = await _feature.CreateModuleAsync(name);
        return res ? true : Problem(_feature.ErrorMsg);
    }

    /// <summary>
    /// 重构Manager接口
    /// </summary>
    /// <returns></returns>
    [HttpPut("removeIManager")]
    public async Task<bool> RemoveIManagerAsync()
    {
        return await UpdateManager.RemoveManagerInterfaceAsync(_projectContext.SolutionPath!);
    }
}
