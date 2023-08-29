using System.Text;
using AterStudio.Advance;
using AterStudio.Models;
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
    private readonly EntityAdvance _entityAdvance;
    public AdvanceController(EntityAdvance entityAdvance)
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
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(message.Content));
                await Response.Body.FlushAsync();
            }
        }
    }

    [HttpGet("test")]
    public async Task Test()
    {
        Response.ContentType = "text/plain";
        Response.StatusCode = StatusCodes.Status200OK;
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(500);
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("content:" + i));
            await Response.Body.FlushAsync();
        }
        //await Response.CompleteAsync();
    }


    /// <summary>
    /// 获取token
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    [HttpGet("token")]
    public async Task<string?> GetTokenAsync(string username, string password)
    {
        return await _entityAdvance.GetTokenAsync(username, password);
    }

    /// <summary>
    /// 生成实体
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    [HttpGet("entity")]
    public async Task<ActionResult<List<string>?>> GetEntityAsync(string name, string description)
    {
        // get token from  httpcontext header 
        var token = HttpContext.Request.Headers["Authorization"].ToString();
        if (token == null) return Forbid("禁止访问");
        var res = await _entityAdvance.GetEntityAsync(name, description, token);
        if (res == null)
        {
            return Problem("获取失败");
        }
        return res;
    }


    /// <summary>
    /// 创建实体
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("entity/{projectId}")]
    public bool CreateEntity([FromRoute] Guid projectId, AddEntityDto dto)
    {
        return _entityAdvance.CreateEntity(dto);

    }
}

