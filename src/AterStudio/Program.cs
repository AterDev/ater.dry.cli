using AterStudio;
using AterStudio.Manager;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ContextBase>(options =>
{
    string? connectionString = builder.Configuration.GetConnectionString("default");
    _ = options.UseSqlite(connectionString);
});
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddScoped<ProjectManager>();
builder.Services.AddScoped<EntityManager>();

// cors配置 
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", builder =>
    {
        _ = builder.AllowAnyOrigin();
        _ = builder.AllowAnyMethod();
        _ = builder.AllowAnyHeader();
    });
});
#if DEBUG
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ater Studio",
        Description = "API 文档",
        Version = "v1"
    });
    string[] xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (string item in xmlFiles)
    {
        try
        {
            c.IncludeXmlComments(item, includeControllerXmlComments: true);
        }
        catch (Exception) { }
    }
    c.DescribeAllParametersInCamelCase();
    c.CustomOperationIds((z) =>
    {
        ControllerActionDescriptor descriptor = (ControllerActionDescriptor)z.ActionDescriptor;
        return $"{descriptor.ControllerName}_{descriptor.ActionName}";
    });
    c.SchemaFilter<EnumSchemaFilter>();
    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
});
#endif

WebApplication app = builder.Build();

// 异常统一处理
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        context.Response.StatusCode = 500;
        Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var result = new
        {
            Title = "程序内部错误:" + exception?.Message,
            Detail = exception?.Source,
            Status = 500,
            TraceId = context.TraceIdentifier
        };
        await context.Response.WriteAsJsonAsync(result);
    });
});

// 初始化
var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<ContextBase>();
await context.Database.MigrateAsync();


app.UseCors("default");
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
