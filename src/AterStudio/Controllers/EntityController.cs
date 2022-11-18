using AterStudio.Entity;
using AterStudio.Manager;
using AterStudio.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<List<EntityFile>>> ListAsync([FromRoute] int id)
    {
        if (!_context.Projects.Any(p => p.Id == id))
        {
            return NotFound("不存在的项目");
        }

        return await _manager.GetEntityFilesAsync(id);
    }
}
