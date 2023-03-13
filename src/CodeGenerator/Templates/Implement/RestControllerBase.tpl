using System.Diagnostics;
using Core.Const;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Http.API.Infrastructure;

/// <summary>
/// 管理后台权限控制器
/// </summary>
[Route("api/admin/[controller]")]
[Authorize(Const.AdminUser)]
[ApiExplorerSettings(GroupName = "admin")]
public class RestControllerBase<TManager> : RestControllerBase
     where TManager : class
{
    protected readonly TManager manager;
    protected readonly ILogger _logger;
    protected readonly IUserContext _user;

    public RestControllerBase(
        TManager manager,
        IUserContext user,
        ILogger logger
        )
    {
        this.manager = manager;
        _user = user;
        _logger = logger;
    }

    protected async Task<SystemUser?> GetUserAsync()
    {
        return await _user.GetSystemUserAsync();
    }

    // TODO:角色权限
    public virtual bool HasPermission()
    {
        return true;
    }

}

/// <summary>
/// 用户端权限控制器
/// </summary>
/// <typeparam name="TManager"></typeparam>
[Authorize(Const.User)]
[ApiExplorerSettings(GroupName = "client")]
public class ClientControllerBase<TManager> : RestControllerBase
     where TManager : class
{
    protected readonly TManager manager;
    protected readonly ILogger _logger;
    protected readonly IUserContext _user;

    public ClientControllerBase(
        TManager manager,
        IUserContext user,
        ILogger logger
        )
    {
        this.manager = manager;
        _user = user;
        _logger = logger;
    }

    protected async Task<User?> GetUserAsync()
    {
        return await _user.GetUserAsync();
    }
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
        var res = new {
            Title = "访问的资源不存在",
            Detail = value?.ToString(),
            Status = 404,
            TraceId = HttpContext.TraceIdentifier
        };
        var at = Activity.Current;
        at?.SetTag("responseBody", value);
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
        var res = new {
            Title = "重复的资源",
            Detail = error?.ToString(),
            Status = 409,
            TraceId = HttpContext.TraceIdentifier
        };
        var at = Activity.Current;
        at?.SetTag("responseBody", error);
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
        var res = new {
            Title = "业务错误",
            Detail = detail,
            Status = 500,
            TraceId = HttpContext.TraceIdentifier
        };
        var at = Activity.Current;
        at?.SetTag("responseBody", detail);
        return new ObjectResult(res)
        {
            StatusCode = 500,

        };
    }
}