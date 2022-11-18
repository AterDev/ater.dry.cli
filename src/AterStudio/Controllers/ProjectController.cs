using AterStudio.Entity;
using AterStudio.Manager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ContextBase _context;

    private ProjectManager _manager;
    public ProjectController(ContextBase context, ProjectManager manager)
    {
        _context = context;
        _manager = manager;
    }

    [HttpGet]
    public async Task<List<Project>> ListAsync()
    {
        return await _context.Projects.ToListAsync();
    }


    [HttpPost]
    public async Task<ActionResult<Project>> AddAsync(string name, string path)
    {
        if (!System.IO.File.Exists(path))
        {
            return Problem("未找到该路径");
        }
        return await _manager.AddProjectAsync(name, path);
    }
}
