using AterStudio.Manager;
using Command.Share;
using Core.Infrastructure;
using Datastore;
using Datastore.Migrations;
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

    /// <summary>
    /// 开户监测
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("watcher/{id}")]
    public async Task<ActionResult<bool>> StartWatcherAsync([FromRoute] int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound("不存在该项目");
        }

        Const.PROJECT_ID = project.ProjectId;
        _manager.StartWatcher(project);
        return true;
    }

    /// <summary>
    /// 停止监测
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("watcher/{id}")]
    public async Task<ActionResult<bool>> StopWatcherAsync([FromRoute] int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return NotFound("不存在该项目");
        }
        Const.PROJECT_ID = project.ProjectId;
        _manager.StopWatcher(project);
        return true;
    }
}
