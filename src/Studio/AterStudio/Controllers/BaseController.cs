using Ater.Web.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Share;

namespace AterStudio.Controllers;

[Route("api/admin/[controller]")]
[ApiExplorerSettings(GroupName = "admin")]
public class BaseController<TManager>(TManager manager, IProjectContext project ,ILogger logger) : RestControllerBase
    where TManager : class
{
    protected readonly TManager _manager = manager;
    protected readonly IProjectContext _project = project;
    protected readonly ILogger _logger = logger;

}
