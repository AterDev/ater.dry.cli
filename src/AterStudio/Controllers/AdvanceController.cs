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


    /// <summary>
    /// 测试AI对话
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [HttpGet("test")]
    public async Task TestAsync(string str)
    {
        //Response.ContentType = "text/text;charset=utf-8";
        //_aiService.BuildKernel("deepSeekApiKey");
        //try
        //{
        //    var results = _aiService.StreamCompletionAsync(str);
        //    await foreach (var result in results)
        //    {
        //        await Response.WriteAsync(result.Content);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.Message);
        //}
        //await Response.CompleteAsync();
    }
}

