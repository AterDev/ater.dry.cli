using AterStudio.Manager;
using AterStudio.Models;
using Command.Share.Commands;
using Core.Entities;
using Datastore.Models;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
///  实体
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntityController(EntityManager manager) : ControllerBase
{
    private readonly EntityManager _manager = manager;

    [HttpGet("{id}")]
    public ActionResult<List<EntityFile>> List([FromRoute] Guid id, string? name)
    {
        return !_manager.IsExist(id)
            ? NotFound("不存在的项目")
            : _manager.GetEntityFiles(name);
    }

    /// <summary>s
    /// 获取dtos
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <returns></returns>
    [HttpGet("dtos")]
    public ActionResult<List<EntityFile>> GetDtos(string entityFilePath)
    {
        if (!System.IO.File.Exists(entityFilePath))
        {
            return NotFound("不存在的文件");
        }
        return _manager.GetDtos(entityFilePath);
    }

    /// <summary>
    /// 创建DTO
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <param name="name"></param>
    /// <param name="summary"></param>
    /// <returns></returns>
    [HttpPost("dto")]
    public async Task<ActionResult<string>> CreateDtoAsync(string entityFilePath, string name, string summary)
    {
        if (!System.IO.File.Exists(entityFilePath))
        {
            return NotFound("不存在的文件");
        }
        var res = await _manager.CreateDtoAsync(entityFilePath, name, summary);
        if (res == null)
        {
            return BadRequest("创建失败");
        }
        return res;
    }

    /// <summary>
    /// 清理解决方案
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public string CleanSolution()
    {
        var res = _manager.CleanSolution(out var errorMsg);
        return res ? "清理成功" : errorMsg;

    }

    /// <summary>
    /// 获取文件内容 entity/manager
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="isManager">是否为manager</param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    [HttpGet("fileContent")]
    public EntityFile? GetFileContent(string entityName, bool isManager, string? moduleName = null)
    {
        return _manager.GetFileContent(entityName, isManager, moduleName);
    }


    /// <summary>
    /// 更新内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("dto")]
    public bool UpdateDtoContent(UpdateDtoDto dto)
    {
        return _manager.UpdateDtoContent(dto.FileName, dto.Content);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<bool>> GenerateAsync(GenerateDto dto)
    {
        Project? project = _manager.Find(dto.ProjectId);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateAsync(project, dto);
        return true;
    }

    /// <summary>
    /// 批量生成
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("batch-generate")]
    public async Task<ActionResult<bool>> BatchGenerateAsync(BatchGenerateDto dto)
    {
        Project? project = _manager.Find(dto.ProjectId);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.BatchGenerateAsync(dto);
        return true;
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
        Project? project = _manager.Find(id);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateRequestAsync(webPath, type, swaggerPath);
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
        Project? project = _manager.Find(id);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateClientRequestAsync(project, webPath, type, swaggerPath);
        return true;
    }


    /// <summary>
    /// 同步ng页面
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("generateSync/{id}")]
    public async Task<ActionResult<bool>> GenerateSync([FromRoute] Guid id)
    {
        Project? project = _manager.Find(id);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateSyncAsync(project);
        return true;
    }

    /// <summary>
    /// 生成NG组件模块
    /// </summary>
    /// <param name="id"></param>
    /// <param name="entityName"></param>
    /// <param name="rootPath"></param>
    /// <returns></returns>
    [HttpPost("generateNgModule/{id}")]
    public async Task<ActionResult<bool>> GenerateNgModuleAsync([FromRoute] Guid id, string entityName, string rootPath)
    {
        Project? project = _manager.Find(id);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateNgModuleAsync(entityName, rootPath);
        return true;
    }


}
