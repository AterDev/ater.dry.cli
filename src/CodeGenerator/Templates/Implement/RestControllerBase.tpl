using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Http.API.Infrastructure;

/// <summary>
/// 管理后台权限控制器
/// </summary>
[Route("api/admin/[controller]")]
[Authorize(AppConst.AdminUser)]
[ApiExplorerSettings(GroupName = "admin")]
public class RestControllerBase<TManager>(
    TManager manager,
    IUserContext user,
    ILogger logger
        ) : RestControllerBase
     where TManager : class
{
    protected readonly TManager manager = manager;
    protected readonly ILogger _logger = logger;
    protected readonly IUserContext _user = user;
}

/// <summary>
/// 用户端权限控制器
/// </summary>
/// <typeparam name="TManager"></typeparam>
[Authorize(AppConst.User)]
[ApiExplorerSettings(GroupName = "client")]
public class ClientControllerBase<TManager>(
    TManager manager,
    IUserContext user,
    ILogger logger
        ) : RestControllerBase
     where TManager : class
{
    protected readonly TManager manager = manager;
    protected readonly ILogger _logger = logger;
    protected readonly IUserContext _user = user;
}

/// <summary>
/// http api 基类，重写ControllerBase中的方法
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RestControllerBase : ControllerBase
{

    /// <summary>
    /// 404返回格式处理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [NonAction]
    public override NotFoundObjectResult NotFound([ActionResultObjectValue] object? value)
    {
        var res = new
        {
            Title = "访问的资源不存在",
            Detail = value?.ToString(),
            Status = 404,
            TraceId = HttpContext.TraceIdentifier
        };
        Activity? at = Activity.Current;
        _ = (at?.SetTag("responseBody", value));
        return base.NotFound(res);
    }

    /// <summary>
    /// 409返回格式处理
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    [NonAction]
    public override ConflictObjectResult Conflict([ActionResultObjectValue] object? error)
    {
        var res = new
        {
            Title = "重复的资源",
            Detail = error?.ToString(),
            Status = 409,
            TraceId = HttpContext.TraceIdentifier
        };
        Activity? at = Activity.Current;
        _ = (at?.SetTag("responseBody", error));
        return base.Conflict(res);
    }

    /// <summary>
    /// 500业务错误
    /// </summary>
    /// <param name="detail"></param>
    /// <returns></returns>
    [NonAction]
    public ObjectResult Problem(string? detail = null)
    {
        var res = new
        {
            Title = "业务错误",
            Detail = detail,
            Status = 500,
            TraceId = HttpContext.TraceIdentifier
        };
        Activity? at = Activity.Current;
        _ = (at?.SetTag("responseBody", detail));
        return new ObjectResult(res)
        {
            StatusCode = 500,

        };
    }
    /// <summary>
    /// 400返回格式处理
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    [NonAction]
    public override BadRequestObjectResult BadRequest([ActionResultObjectValue] object? error)
    {
        var res = new
        {
            Title = "请求错误",
            Detail = error?.ToString(),
            Status = 400,
            TraceId = HttpContext.TraceIdentifier
        };
        return base.BadRequest(res);
    }
}