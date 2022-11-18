using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly ContextBase _context;
    public ProjectController(ContextBase context)
    {
        _context = context;
    }
}
