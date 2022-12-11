using AterStudio.Manager;
using Datastore;
using Datastore.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ContextBase _context;

    private readonly ProjectManager _manager;
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
    public async Task<ActionResult<Project?>> AddAsync(string name, string path)
    {
        return !System.IO.File.Exists(path) ? Problem("未找到该路径") : await _manager.AddProjectAsync(name, path);
    }
}
