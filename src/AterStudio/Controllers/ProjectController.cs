using AterStudio.Advance;
using AterStudio.Manager;
using AterStudio.Models;
using Core.Entities;
using Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 项目
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ProjectManager _manager;
    private readonly EntityAdvance _advace;
    public ProjectController(ProjectManager manager, EntityAdvance advace)
    {
        _manager = manager;
        _advace = advace;
    }

    [HttpGet]
    public List<Project> List()
    {
        return _manager.GetProjects();
    }

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public Project? Project([FromRoute] Guid id)
    {
        return _manager.GetProject(id);
    }

    [HttpGet("sub/{id}")]
    public List<SubProjectInfo>? GetAllProjectInfos([FromRoute] Guid id)
    {
        return _manager.GetAllProjects(id);
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Project?>> AddAsync(string name, string path)
    {
        return (!System.IO.File.Exists(path) && !Directory.Exists(path))
            ? Problem("未找到该路径")
            : await _manager.AddProjectAsync(name, path);
    }

    /// <summary>
    /// 获取项目配置文件内容
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("setting/{id}")]
    public async Task<ActionResult<ConfigOptions>> GetConfigOptionsAsync([FromRoute] Guid id)
    {
        var config = await _manager.GetConfigOptions(id);
        return config == null ? Problem("配置文件加载失败") : config;
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("setting/{id}")]
    public async Task<bool> UpdateConfigAsync([FromRoute] Guid id, UpdateConfigOptionsDto dto)
    {
        return await _manager.UpdateConfigAsync(id, dto);
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public ActionResult<bool> Delete([FromRoute] Guid id)
    {
        return _manager.DeleteProject(id);
    }


    /// <summary>
    /// 获取模板名称
    /// </summary>
    /// <returns></returns>
    [HttpGet("tempaltes/{id}")]
    public List<TemplateFile> GetTemplateFiles([FromRoute] Guid id)
    {
        return _manager.GetTemplateFiles(id);
    }


    /// <summary>
    /// 获取模板内容
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpGet("template/{id}")]
    public TemplateFile GetTemplateFile([FromRoute] Guid id, string name)
    {
        return _manager.GetTemplate(id, name);
    }

    /// <summary>
    /// 更新模板内容
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("template/{id}")]
    public bool SaveTemplateFile([FromRoute] Guid id, TemplateFileUpsert dto)
    {
        return _manager.SaveTemplate(id, dto.Name, dto.Content);
    }

    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("database/{id}")]
    public string GetDatabaseContent([FromRoute] Guid id)
    {
        return _advace.GetDatabaseStructure(id);
    }

    /// <summary>
    /// 获取监听状态
    /// </summary>
    /// <returns></returns>
    [HttpGet("watcher/{id}")]
    public ActionResult<bool> GetWatcherStatus([FromRoute] Guid id)
    {
        var project = _manager.GetProject(id);
        if (project == null)
        {
            return NotFound("不存在该项目");
        }
        return _manager.GetWatcherStatus(project);
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
