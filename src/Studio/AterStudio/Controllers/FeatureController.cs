using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 功能模块
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeatureController(FeatureManager feature, ProjectContext projectContext) : ControllerBase
{
    private readonly FeatureManager _feature = feature;
    private readonly ProjectContext _projectContext = projectContext;

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    [HttpPost("newSolution")]
    public async Task<ActionResult<bool>> CreateNewSolution(CreateSolutionDto dto)
    {
        bool res = await _feature.CreateNewSolutionAsync(dto);
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
    /// 获取默认模块
    /// </summary>
    /// <returns></returns>
    [HttpGet("defaultModules")]
    public List<ModuleInfo> GetDefaultModules()
    {
        return _feature.GetDefaultModules();
    }

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost("createModule")]
    public async Task<ActionResult<bool>> CreateModule(string name)
    {
        bool res = await _feature.CreateModuleAsync(name);
        return res ? true : Problem(_feature.ErrorMsg);
    }
}
