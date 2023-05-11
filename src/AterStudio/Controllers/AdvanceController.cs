using AterStudio.Advance;
using AterStudio.Models;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

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
        return _entityAdvance.CreateEntity(projectId, dto);

    }
}

