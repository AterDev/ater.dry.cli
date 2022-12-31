using AterStudio.Manager;
using Datastore.Models;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

/// <summary>
/// api文档
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApiDocController : ControllerBase
{
    private readonly ApiDocManager _manager;
    private readonly ProjectManager _project;
    public ApiDocController(ApiDocManager manager, ProjectManager project)
    {
        _manager = manager;
        _project = project;
    }


    /// <summary>
    /// 获取项目文档
    /// </summary>
    /// <param name="id">项目id</param>
    /// <returns></returns>
    [HttpGet("all/{id}")]
    public ActionResult<List<ApiDocInfo>> List([FromRoute] Guid id)
    {
        var project = _project.GetProject(id);
        return project == null
            ? NotFound("不存在的项目")
            : _manager.FindAll(project);
    }

    /// <summary>
    /// 获取某个文档信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public ActionResult<ApiDocInfo?> GetApiDocContent([FromRoute] Guid id)
    {
        // TODO:
        return _manager.Find(id);
    }


    /// <summary>
    ///  添加
    /// </summary>
    /// <param name="apiDocInfo"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult<ApiDocInfo> Add(ApiDocInfo apiDocInfo)
    {
        return _manager.AddApiDoc(apiDocInfo);
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public bool Delete([FromRoute] Guid id)
    {
        return _manager.Delete(id);
    }


}
