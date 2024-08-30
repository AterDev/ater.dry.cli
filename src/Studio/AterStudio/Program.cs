using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using Application;

using Ater.Web.Abstraction;

using AterStudio;
using AterStudio.Worker;
using CodeGenerator.Helper;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Share.EntityFramework.DBProvider;

Console.OutputEncoding = Encoding.UTF8;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProjectContext>();

var path = Path.Combine(AssemblyHelper.GetStudioPath(), ContextBase.DbName);
builder.Services.AddDbContext<CommandDbContext>(options =>
{
    options.UseSqlite($"DataSource={path}", _ =>
    {
        _.MigrationsAssembly("AterStudio");
    });
});

builder.Services.AddDbContext<QueryDbContext>(options =>
{
    options.UseSqlite($"DataSource={path}", _ =>
    {
        _.MigrationsAssembly("AterStudio");
    });
});

builder.Services.AddManager();
builder.Services.AddSingleton(typeof(AIService));

#if DEBUG 
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("client", new OpenApiInfo
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
    c.SupportNonNullableReferenceTypes();
    c.SchemaFilter<EnumSchemaFilter>();
    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
});
#endif
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        o.InvalidModelStateResponseFactory = context =>
        {
            return new CustomBadRequest(context, null);
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    });

WebApplication app = builder.Build();
app.UseWebAppContext();

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
            Detail = exception?.Message + exception?.StackTrace,
            Status = 500,
            TraceId = context.TraceIdentifier
        };
        await context.Response.WriteAsJsonAsync(result);
    });
});

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapFallbackToFile("index.html");

using (app)
{
    var scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

