using Ater.Web.Abstraction;

using Definition.Share.Models.EntityInfoDtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
///  实体
/// </summary>
[AllowAnonymous]
public class EntityInfoController(
    EntityInfoManager manager,
    ProjectContext project,
    ILogger<EntityInfoController> logger
    ) : RestControllerBase
{
    private readonly EntityInfoManager manager = manager;
    private readonly ProjectContext _project = project;
    private readonly ILogger<EntityInfoController> logger = logger;

    [HttpGet("{id}")]
    public ActionResult<List<EntityFile>> List([FromRoute] Guid id, string? serviceName)
    {
        return _project.Project == null
            ? NotFound("不存在的项目")
            : manager.GetEntityFiles(serviceName);
    }

    /// <summary>s
    /// 获取dtos
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <returns></returns>
    [HttpGet("dtos")]
    public ActionResult<List<EntityFile>> GetDtos(string entityFilePath)
    {
        return !System.IO.File.Exists(entityFilePath) ? NotFound("不存在的文件") : manager.GetDtos(entityFilePath);
    }

    /// <summary>
    /// 创建DTO
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <param name="name"></param>
    /// <param name="summary"></param>
    /// <returns></returns>
    [HttpPost("dto")]
    public async Task<ActionResult<string>> CreateDtoAsync(string entityFilePath, string name, string summary)
    {
        if (!System.IO.File.Exists(entityFilePath))
        {
            return NotFound("不存在的文件");
        }
        var res = await manager.CreateDtoAsync(entityFilePath, name, summary);
        return res == null ? BadRequest("创建失败") : res;
    }

    /// <summary>
    /// 清理解决方案
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public string CleanSolution()
    {
        var res = manager.CleanSolution(out var errorMsg);
        return res ? "清理成功" : errorMsg;

    }

    /// <summary>
    /// 获取文件内容 entity/manager
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="isManager">是否为manager</param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    [HttpGet("fileContent")]
    public EntityFile? GetFileContent(string entityName, bool isManager, string? moduleName = null)
    {
        return manager.GetFileContent(entityName, isManager, moduleName);
    }


    /// <summary>
    /// 更新内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("dto")]
    public bool UpdateDtoContent(UpdateDtoDto dto)
    {
        return manager.UpdateDtoContent(dto.FileName, dto.Content);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<bool>> GenerateAsync(GenerateDto dto)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }
        await manager.GenerateAsync(_project.Project, dto);
        return true;
    }

    /// <summary>
    /// 批量生成
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("batch-generate")]
    public async Task<ActionResult<bool>> BatchGenerateAsync(BatchGenerateDto dto)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }
        await manager.BatchGenerateAsync(dto);
        return true;
    }

    /// <summary>
    /// 同步ng页面
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("generateSync/{id}")]
    public async Task<ActionResult<bool>> GenerateSync([FromRoute] Guid id)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }
        await manager.GenerateSyncAsync(_project.Project);
        return true;
    }

    /// <summary>
    /// 生成NG组件模块
    /// </summary>
    /// <returns></returns>
    [HttpPost("generateNgModule")]
    public async Task<ActionResult<bool>> GenerateNgModuleAsync(NgModuleDto dto)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }

        await manager.GenerateNgModuleAsync(dto.EntityName, dto.RootPath, dto.IsMobile);
        return true;
    }


}
