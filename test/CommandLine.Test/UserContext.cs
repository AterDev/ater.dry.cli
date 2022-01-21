using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CommandLine.Test;

public class UserContext : IUserContext
{
    public Guid? UserId { get; init; }
    public Guid? SessionId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; set; }
    public bool IsAdmin { get; init; } = false;
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }
    public Guid? GroupId { get; init; }
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        if (Guid.TryParse(FindClaim(ClaimTypes.NameIdentifier)?.Value, out var userId) && userId != Guid.Empty)
        {
            UserId = userId;
        }
        if (Guid.TryParse(FindClaim(ClaimTypes.GroupSid)?.Value, out var groupSid) && groupSid != Guid.Empty)
        {
            GroupId = groupSid;
        }
        Username = FindClaim(ClaimTypes.Name)?.Value;
        Email = FindClaim(ClaimTypes.Email)?.Value;
        CurrentRole = FindClaim(ClaimTypes.Role)?.Value;
        if (CurrentRole != null && CurrentRole.ToLower() == "admin")
        {
            IsAdmin = true;
        }
        else
        {
            IsAdmin = false;
        }
    }

    public Claim? FindClaim(string claimType) => _httpContextAccessor?.HttpContext?.User?.FindFirst(claimType);

}
