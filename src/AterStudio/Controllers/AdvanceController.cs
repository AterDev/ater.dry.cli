using AterStudio.Advance;

using Definition.Entity;

using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 高级功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdvanceController(AdvanceManager entityAdvance) : ControllerBase
{
    private readonly AdvanceManager _entityAdvance = entityAdvance;


    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("config")]
    public ActionResult<ConfigData> GetConfig(string key)
    {
        var data = _entityAdvance.GetConfig(key);
        return data != null ? (ActionResult<ConfigData>)data : (ActionResult<ConfigData>)Ok();
    }

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut("config")]
    public ActionResult SetConfig(string key, string value)
    {
        _entityAdvance.SetConfig(key, value);
        return Ok();
    }
}

