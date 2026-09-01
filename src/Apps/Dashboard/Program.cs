using CoreMod.Services;
using Dashboard.Components.Pages;
using Microsoft.AspNetCore.Localization;
using Share.Helper;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "⏱️ HH:mm:ss ";
});

builder.AddFrameworkServices();
builder.AddMiddlewareServices();
builder.AddBlazorServices();

builder.Services.AddScoped<SolutionManager>();
builder.Services.AddScoped<EntityInfoManager>();
builder.Services.AddScoped<ApiDocInfoManager>();
builder.Services.AddScoped<ToolsManager>();

// services
builder.Services.AddScoped<SolutionContext, SolutionContext>();
builder.Services.AddScoped<CodeAnalysisService>();
builder.Services.AddScoped<CodeGenService>();
builder.Services.AddScoped<CommandService>();
builder.Services.AddScoped<SolutionService>();
builder.Services.AddScoped<OfficialModuleService>();
builder.Services.AddScoped<ModuleInstallService>();
builder.Services.AddSingleton<StorageService>();


WebApplication app = builder.Build();
app.UseMiddlewareServices();


var dir = AssemblyHelper.GetStudioPath();
var path = Path.Combine(dir, ConstVal.DbName);
OutputHelper.Info("using db file:" + path);


// 使用 Minimal API 处理语言切换
app.MapGet("/Culture/SetCulture", (string culture, string? redirectUri, HttpContext context) =>
{
    if (string.IsNullOrWhiteSpace(culture))
    {
        culture = "zh-CN";
    }

    if (string.IsNullOrWhiteSpace(redirectUri))
    {
        redirectUri = "/";
    }

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(
            new RequestCulture(culture, culture)),
        new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        }
    );

    return Results.LocalRedirect(redirectUri);
});

app.Run();


