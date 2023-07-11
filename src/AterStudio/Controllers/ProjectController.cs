using AterStudio.Advance;
using AterStudio.Manager;
using AterStudio.Models;
using Core.Entities;
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

    /// <summary>
    /// 获取解决方案列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public List<Project> List()
    {
        return _manager.GetProjects();
    }

    /// <summary>
    /// 获取工具版本
    /// </summary>
    /// <returns></returns>
    [HttpGet("verison")]
    public string GetVersion()
    {
        return _manager.GetToolVersion();
    }

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<Project?> ProjectAsync([FromRoute] Guid id)
    {
        return await _manager.GetProjectAsync(id);
    }

    [HttpGet("sub/{id}")]
    public async Task<List<SubProjectInfo>?> GetAllProjectInfosAsync([FromRoute] Guid id)
    {
        return await _manager.GetAllProjectsAsync(id);
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
    /// <returns></returns>
    [HttpGet("setting")]
    public ActionResult<ConfigOptions> GetConfigOptions()
    {
        var config = _manager.GetConfigOptions();
        return config == null ? Problem("配置文件加载失败") : config;
    }

    /// <summary>
    /// 更新解决方案
    /// </summary>
    /// <returns></returns>
    [HttpPut("solution")]
    public async Task<string> UpdateSolutionAsync()
    {
        return await _manager.UpdateSolutionAsync();
    }


    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("setting")]
    public async Task<bool> UpdateConfigAsync(UpdateConfigOptionsDto dto)
    {
        return await _manager.UpdateConfigAsync(dto);
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
    public async Task<ActionResult<bool>> GetWatcherStatusAsync([FromRoute] Guid id)
    {
        var project = await _manager.GetProjectAsync(id);
        if (project == null)
        {
            return NotFound("不存在该项目");
        }
        return _manager.GetWatcherStatus(project);
    }

}
