using Application;

using Definition.Entity;

using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 高级功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdvanceController(AdvanceManager entityAdvance, AIService aiService) : ControllerBase
{
    private readonly AdvanceManager _entityAdvance = entityAdvance;
    private readonly AIService _aiService = aiService;

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("config")]
    public ActionResult<ConfigData> GetConfig(string key)
    {
        var data = _entityAdvance.GetConfig(key);
        return data != null ? data : Ok();
    }

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut("config")]
    public async Task<ActionResult> SetConfigAsync(string key, string value)
    {
        await _entityAdvance.SetConfigAsync(key, value);
        return Ok();
    }

    [HttpGet("test")]
    public async Task<ActionResult<string>> TestAsync(string str)
    {
        _aiService.BuildKernel("deepSeekApiKey");
        var res = await _aiService.TestAsync(str);
        return Ok(res);
    }
}

