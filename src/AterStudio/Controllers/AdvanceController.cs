using AterStudio.Advance;
using Microsoft.AspNetCore.Mvc;

namespace AterStudio.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvanceController : ControllerBase
{
    private readonly EntityAdvance _entityAdvance;
    public AdvanceController(EntityAdvance entityAdvance)
    {
        _entityAdvance = entityAdvance;
    }


    [HttpGet("token")]
    public async Task<string?> GetTokenAsync(string username, string password)
    {
        return await _entityAdvance.GetTokenAsync(username, password);
    }


    [HttpGet("entity")]
    public async Task<List<string>?> GetEntityAsync(string name, string description)
    {
        return await _entityAdvance.GetEntityAsync(name, description);
    }
}

