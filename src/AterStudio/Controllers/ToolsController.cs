using AterStudio.Manager;
using AterStudio.Models;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;
/// <summary>
/// Tools
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToolsController : ControllerBase
{
    private readonly ToolsManager _toolsManager;


    public ToolsController(ToolsManager toolsManager)
    {
        _toolsManager = toolsManager;
    }

    /// <summary>
    /// 转换成类
    /// </summary>
    /// <returns></returns>
    [HttpPost("classModel")]
    public ActionResult<List<string>?> ConvertToClass([FromBody] ConvertDto dto)
    {
        var res = _toolsManager.ConvertToClass(dto.Content);
        if (res == null)
        {
            return Problem("未能转换成功,请输入合法的json");
        }

        return res;

    }
}
