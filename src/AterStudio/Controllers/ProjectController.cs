using AterStudio.Manager;
using Core.Infrastructure;
using Datastore;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ProjectManager _manager;
    public ProjectController(ProjectManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public List<Project> List()
    {
        return _manager.GetProjects();
    }

    [HttpPost]
    public async Task<ActionResult<Project?>> AddAsync(string name, string path)
    {
        return !System.IO.File.Exists(path) ? Problem("未找到该路径") : await _manager.AddProjectAsync(name, path);
    }

    /// <summary>
    /// 开启监测
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("watcher/{id}")]
    public ActionResult<bool> StartWatcher([FromRoute] Guid id)
    {
        var project = _manager.GetProject(id);
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
    public ActionResult<bool> StopWatcher([FromRoute] Guid id)
    {
        var project = _manager.GetProject(id);
        if (project == null)
        {
            return NotFound("不存在该项目");
        }
        Const.PROJECT_ID = project.ProjectId;
        _manager.StopWatcher(project);
        return true;
    }
}
