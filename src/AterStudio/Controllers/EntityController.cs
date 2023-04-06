using AterStudio.Manager;
using Command.Share.Commands;
using Datastore.Models;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
///  实体
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntityController : ControllerBase
{
    private readonly EntityManager _manager;
    public EntityController(EntityManager manager)
    {
        _manager = manager;
    }

    [HttpGet("{id}")]
    public ActionResult<List<EntityFile>> List([FromRoute] Guid id, string? name)
    {
        return !_manager.IsExist(id)
            ? NotFound("不存在的项目")
            : _manager.GetEntityFiles(id, name);
    }

    /// <summary>
    /// 获取dtos
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="entityName"></param>
    /// <returns></returns>
    [HttpGet("dtos")]
    public List<EntityFile> GetDtos(Guid projectId, string entityName)
    {
        return _manager.GetDtos(projectId, entityName);
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
        await _manager.BatchGenerateAsync(project, dto);
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
        await _manager.GenerateRequestAsync(project, webPath, type, swaggerPath);
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
}
