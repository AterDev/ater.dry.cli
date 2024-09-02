using Application;
using Ater.Web.Abstraction;
using Entity;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 项目
/// </summary>
/// <see cref="ProjectManager"/>
public class ProjectController(
    ProjectManager manager,
    AdvanceManager advance,
    IUserContext user,
    ILogger<ProjectContext> logger) : RestControllerBase<ProjectManager>(manager, user, logger)
{
    private readonly ProjectManager _manager = manager;
    private readonly AdvanceManager _advance = advance;

    /// <summary>
    /// 获取解决方案列表
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<Project>> ListAsync()
    {
        return await _manager.ListAsync();
    }

    /// <summary>
    /// 获取工具版本
    /// </summary>
    /// <returns></returns>
    [HttpGet("version")]
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
        return await _manager.GetDetailAsync(id);
    }

    /// <summary>
    /// 添加项目
    /// </summary>
    /// <param name="name"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(string name, string path)
    {
        if (!Directory.Exists(path))
        {
            return Problem("未找到该路径");
        }

        string? projectFilePath = Directory.GetFiles(path, $"*{Const.SolutionExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();

        projectFilePath ??= Directory.GetFiles(path, $"*{Const.CSharpProjectExtension}", SearchOption.TopDirectoryOnly).FirstOrDefault();
        projectFilePath ??= Directory.GetFiles(path, Const.NodeProjectFile, SearchOption.TopDirectoryOnly).FirstOrDefault();

        if (projectFilePath == null)
        {
            return Problem("Not Found valid Project!");
        }
        return await _manager.AddAsync(name, path);
    }

    /// <summary>
    /// 添加微服务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost("service")]
    public ActionResult<bool> AddService(string name)
    {
        bool res = _manager.AddServiceProject(name);
        return res;
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
    /// 打开解决方案，仅支持sln
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [HttpPost("open")]
    public ActionResult<string> OpenSolution(string path)
    {
        return path.EndsWith(".sln") ? (ActionResult<string>)_manager.OpenSolution(path) : (ActionResult<string>)Problem("不支持的解决方案文件");
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("setting/{id}")]
    public async Task<ActionResult<bool>> UpdateConfigAsync([FromRoute] Guid id, ProjectConfig dto)
    {
        if (!await _manager.ExistAsync(id))
        {
            return NotFound();
        }
        return await _manager.UpdateConfigAsync(id, dto);
    }

    /// <summary>
    /// 删除项目
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return await _manager.DeleteAsync([id], false);
    }

    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("database/{id}")]
    public string GetDatabaseContentAsync([FromRoute] Guid id)
    {
        return _advance.GetDatabaseStructureAsync();
    }
}
