using AterStudio.Manager;
using Datastore;
using Datastore.Models;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntityController : ControllerBase
{
    private readonly ContextBase _context;

    private readonly EntityManager _manager;
    public EntityController(ContextBase context, EntityManager manager)
    {
        _context = context;
        _manager = manager;
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<List<EntityFile>>> ListAsync([FromRoute] int id, string? name)
    {
        return !_context.Projects.Any(p => p.Id == id)
            ? NotFound("不存在的项目")
            : await _manager.GetEntityFilesAsync(id, name);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<bool>> GenerateAsync(GenerateDto dto)
    {
        Project? project = await _context.Projects.FindAsync(dto.ProjectId);
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
        Project? project = await _context.Projects.FindAsync(dto.ProjectId);
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
    /// <returns></returns>
    [HttpGet("generateRequest/{id}")]
    public async Task<ActionResult<bool>> GenerateRequest([FromRoute] int id, string webPath, RequestLibType type)
    {
        Project? project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateRequestAsync(project, webPath, type);
        return true;
    }

    /// <summary>
    /// 同步ng页面
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("generateSync/{id}")]
    public async Task<ActionResult<bool>> GenerateSync([FromRoute] int id)
    {
        Project? project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateSyncAsync(project);
        return true;
    }


}
