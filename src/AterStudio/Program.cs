using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using AterStudio;
using AterStudio.Advance;
using AterStudio.Manager;

using Datastore;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

Console.OutputEncoding = Encoding.UTF8;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ProjectContext>();
builder.Services.AddSingleton<DbContext>();
builder.Services.AddScoped<ProjectManager>();
builder.Services.AddScoped<EntityAdvance>();
builder.Services.AddScoped<EntityManager>();
builder.Services.AddScoped<SwaggerManager>();
builder.Services.AddHttpClient<DusiHttpClient>();

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
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    });
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
            Detail = exception?.Message + exception?.StackTrace,
            Status = 500,
            TraceId = context.TraceIdentifier
        };
        await context.Response.WriteAsJsonAsync(result);
    });
});

// 初始化
IServiceScope scope = app.Services.CreateScope();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("default");
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
