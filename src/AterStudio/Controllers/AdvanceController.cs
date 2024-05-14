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
    /// <param name="prompt"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("chat")]
    public async Task ChatAsync(string prompt, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/text;charset=utf-8";
        _aiService.SetApiKey("deepSeekApiKey");
        try
        {
            var choices = await _aiService.GetAnswerAsync(prompt, cancellationToken);
            if (choices != null)
            {
                await foreach (var choice in choices)
                {
                    await Response.WriteAsync(choice.Delta!.Content);
                }
            }
        }
        catch (Exception ex)
        {
            await Response.WriteAsync("暂时无法提供服务" + ex.Message);
        }
        await Response.CompleteAsync();
    }

    /// <summary>
    /// 清除对话
    /// </summary>
    [HttpDelete("chat")]
    public void ClearChat()
    {
        _aiService.CacheMessages.Clear();
    }
}

