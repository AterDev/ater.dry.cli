using AterStudio.Advance;
using AterStudio.Manager;
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
    private readonly EntityAdvance _advace;
    private readonly FeatureManager _feature;
    public FeatureController(ProjectManager manager, EntityAdvance advace, FeatureManager feature)
    {
        _manager = manager;
        _advace = advace;
        _feature = feature;
    }

    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPost("newSolution")]
    public async Task<ActionResult<bool>> CreateNewSolution(string name, string path)
    {
        var res = await _feature.CreateNewSolutionAsync(name, path);
        return res ? true : Problem(_feature.ErrorMsg);
    }
}
