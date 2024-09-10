using Ater.Web.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

[Route("api/admin/[controller]")]
[ApiExplorerSettings(GroupName = "admin")]
public class BaseController<TManager>(TManager manager, ILogger logger) : RestControllerBase
    where TManager : class
{
    protected readonly TManager _manager = manager;
    protected readonly ILogger _logger = logger;
}
