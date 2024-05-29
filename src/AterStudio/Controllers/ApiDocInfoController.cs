using System.Text;

using Ater.Web.Abstraction;

using Definition.Entity;
using Definition.Models;
using Definition.Share.Models.ApiDocInfoDtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// api文档
/// </summary>
[AllowAnonymous]
public class ApiDocInfoController(
    ApiDocInfoManager manager,
    ProjectContext project,
    ILogger<ApiDocInfoController> logger
    ) : RestControllerBase()
{
    private readonly ProjectContext _project = project;
    private readonly ILogger<ApiDocInfoController> logger = logger;

    /// <summary>
    /// 获取项目文档
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<ApiDocInfoItemDto>>> ListAsync()
    {
        var filter = new ApiDocInfoFilterDto { PageSize = 999, ProjectId = _project.ProjectId };
        var pager = await manager.FilterAsync(filter);
        return pager.Data;
    }

    /// <summary>
    /// 获取某个文档信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isFresh"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiDocContent?>> GetApiDocContentAsync([FromRoute] Guid id, bool isFresh = true)
    {
        var res = await manager.GetContentAsync(id, isFresh);
        if (res == null)
        {
            return Problem(manager.ErrorMsg);
        }
        return res;
    }

    /// <summary>
    /// 导出markdown文档
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("export/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<ActionResult> ExportAsync([FromRoute] Guid id)
    {
        var content = await manager.ExportDocAsync(id);
        return File(Encoding.UTF8.GetBytes(content), "application/octet-stream", "api-doc.md");
    }


    /// <summary>
    ///  添加
    /// </summary>
    /// <param name="apiDocInfo"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiDocInfo>> AddAsync(ApiDocInfoAddDto apiDocInfo)
    {
        var entity = await manager.CreateNewEntityAsync(apiDocInfo);
        return await manager.AddAsync(entity);
    }

    /// <summary>
    ///  更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiDocInfo>> UpdateAsync([FromRoute] Guid id, ApiDocInfoUpdateDto dto)
    {
        var entity = await manager.GetCurrentAsync(id);
        if (entity == null)
        {
            return NotFound("未找到该对象");
        }
        return await manager.UpdateAsync(entity, dto);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiDocInfo?>> Delete([FromRoute] Guid id)
    {
        var entity = await manager.FindAsync(id);
        if (entity == null)
        {
            return NotFound("未找到该对象");
        }
        return await manager.DeleteAsync(entity);
    }

    /// <summary>
    /// 生成页面组件
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("component")]
    public NgComponentInfo CreateUIComponent(CreateUIComponentDto dto)
    {
        return manager.CreateUIComponent(dto);
    }

    /// <summary>
    /// 生成前端请求
    /// </summary>
    /// <param name="id"></param>
    /// <param name="webPath"></param>
    /// <param name="type"></param>
    /// <param name="swaggerPath"></param>
    /// <returns></returns>
    [HttpGet("generateRequest/{id}")]
    public async Task<ActionResult<bool>> GenerateRequest([FromRoute] Guid id, string webPath, RequestLibType type, string? swaggerPath = null)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }

        var entity = await manager.GetCurrentAsync(id);
        if (entity == null)
        {
            return NotFound("未找到文档配置");
        }

        await manager.GenerateRequestAsync(entity, webPath, type, swaggerPath);
        return true;
    }

    /// <summary>
    /// 生成客户端请求
    /// </summary>
    /// <param name="id"></param>
    /// <param name="webPath"></param>
    /// <param name="type"></param>
    /// <param name="swaggerPath"></param>
    /// <returns></returns>
    [HttpGet("generateClientRequest/{id}")]
    public async Task<ActionResult<bool>> GenerateClientRequest([FromRoute] Guid id, string webPath, LanguageType type, string? swaggerPath = null)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }
        var entity = await manager.GetCurrentAsync(id);
        if (entity == null)
        {
            return NotFound("未找到文档配置");
        }

        await manager.GenerateClientRequestAsync(entity, webPath, type, swaggerPath);
        return true;
    }
}
