namespace ${Namespace}.Infrastructure;

/// <summary>
/// http api 基类，重写ControllerBase中的方法
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize("User")]
public class RestControllerBase<TManager> : ControllerBase
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

    /// <summary>
    /// 404返回格式处理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [ApiExplorerSettings(IgnoreApi = true)]
    public override NotFoundObjectResult NotFound([ActionResultObjectValue] object? value)
    {
        var res = new
        {
            Title = "访问的资源不存在",
            Detail = value?.ToString(),
            Status = 404,
            TraceId = HttpContext.TraceIdentifier
        };
        return base.NotFound(res);
    }

    /// <summary>
    /// 409返回格式处理
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    [ApiExplorerSettings(IgnoreApi = true)]
    public override ConflictObjectResult Conflict([ActionResultObjectValue] object? error)
    {
        var res = new
        {
            Title = "重复的资源",
            Detail = error?.ToString(),
            Status = 409,
            TraceId = HttpContext.TraceIdentifier
        };
        return base.Conflict(res);
    }

}
