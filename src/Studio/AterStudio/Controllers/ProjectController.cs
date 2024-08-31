using Ater.Web.Abstraction;

using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// 项目
/// </summary>
/// <see cref="ProjectManager"/>
public class ProjectController(ProjectManager manager, AdvanceManager advance) : RestControllerBase
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
        return await _manager.GetProjectsAsync();
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
    public async Task<ActionResult<string?>> AddAsync(string name, string path)
    {
        if (!Directory.Exists(path))
        {
            return Problem("未找到该路径");
        }
        string? res = await _manager.AddProjectAsync(name, path);
        return res != null ? Problem(res) : Ok("添加成功");
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
    /// 获取项目配置文件内容
    /// </summary>
    /// <returns></returns>
    [HttpGet("setting")]
    public ActionResult<ConfigOptions> GetConfigOptions()
    {
        ConfigOptions? config = _manager.GetConfigOptions();
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
    public string GetDatabaseContentAsync([FromRoute] Guid id)
    {
        return _advance.GetDatabaseStructureAsync();
    }

}
