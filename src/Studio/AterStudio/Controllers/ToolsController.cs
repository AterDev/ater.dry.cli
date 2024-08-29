using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;
/// <summary>
/// Tools
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToolsController(ToolsManager toolsManager) : ControllerBase
{
    private readonly ToolsManager _toolsManager = toolsManager;

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

    /// <summary>
    /// 字符串处理
    /// </summary>
    /// <param name="content"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("string")]
    public ActionResult<Dictionary<string, string>> ConvertString(string content, StringConvertType type)
    {
        var res = _toolsManager.ConvertString(content, type);
        return res;
    }
}
