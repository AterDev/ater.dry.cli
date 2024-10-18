using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AterStudio.Middlewares;

public class DebugAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public DebugAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, new Guid().ToString()),
            new Claim(ClaimTypes.Role, "AdminUser")
        };
        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        context.User = new ClaimsPrincipal(identity);
        await _next(context);
    }
}

public static class DebugAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseDebugAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<DebugAuthorizationMiddleware>();
    }
}
