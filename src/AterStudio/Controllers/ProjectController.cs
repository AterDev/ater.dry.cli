using AterStudio.Manager;
using Core.Infrastructure;
using Datastore;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly DbContext _context;

    private readonly ProjectManager _manager;
    public ProjectController(DbContext context, ProjectManager manager)
    {
        _context = context;
        _manager = manager;
    }

    [HttpGet]
    public List<Project> List()
    {
        return _context.Projects.FindAll().ToList();
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
    public ActionResult<bool> StartWatcher([FromRoute] string id)
    {
        var project = _context.Projects.FindById(id);
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
    public ActionResult<bool> StopWatcher([FromRoute] string id)
    {
        var project = _context.Projects.FindById(id);
        if (project == null)
        {
            return NotFound("不存在该项目");
        }
        Const.PROJECT_ID = project.ProjectId;
        _manager.StopWatcher(project);
        return true;
    }
}
