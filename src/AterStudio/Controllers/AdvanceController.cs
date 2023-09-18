using System.Text;

using AterStudio.Advance;

using Azure.AI.OpenAI;

using Core.Entities;

using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 高级功能
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdvanceController : ControllerBase
{
    private readonly AdvanceManager _entityAdvance;
    public AdvanceController(AdvanceManager entityAdvance)
    {
        _entityAdvance = entityAdvance;
    }


    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("config")]
    public ActionResult<ConfigData> GetConfig(string key)
    {
        var data = _entityAdvance.GetConfig(key);
        if (data != null)
        {
            return data;
        }
        else
        {
            return Ok();
        }
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


    /// <summary>
    /// 生成实体
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    [HttpPost("generateEntity")]
    public async Task GenerateEntity(string content)
    {
        var res = await _entityAdvance.GenerateEntityAsync(content);
        if (res == null)
        {
            Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }
        Response.ContentType = "text/plain";
        Response.StatusCode = StatusCodes.Status200OK;
        await foreach (StreamingChatChoice choice in res.Value.GetChoicesStreaming())
        {
            await foreach (ChatMessage message in choice.GetMessageStreaming())
            {
                if (message.Content == null)
                {
                    continue;
                }
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message.Content));
                await Response.Body.FlushAsync();
            }
        }
    }

    /// <summary>
    /// 问答
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    [HttpGet("answer")]
    public async Task GetAnswerAsync(string content)
    {
        var res = await _entityAdvance.GenerateAnswerAsync(content);
        if (res == null)
        {
            Response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        Response.ContentType = "text/plain";
        Response.StatusCode = StatusCodes.Status200OK;
        await foreach (StreamingChatChoice choice in res.Value.GetChoicesStreaming())
        {
            await foreach (ChatMessage message in choice.GetMessageStreaming())
            {
                if (message.Content == null)
                {
                    continue;
                }
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message.Content));
                await Response.Body.FlushAsync();
            }
        }
    }


    /// <summary>
    /// 生成图片
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    [HttpGet("Images")]
    public async Task<List<string>?> GetImages(string content)
    {
        return await _entityAdvance.GenerateImagesAsync(content);
    }
}

