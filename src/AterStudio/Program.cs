using AterStudio;
using AterStudio.Manager;
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

// corsÅäÖÃ 
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", builder =>
    {
        _ = builder.AllowAnyOrigin();
        _ = builder.AllowAnyMethod();
        _ = builder.AllowAnyHeader();
    });
});
builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyProjectName",
        Description = "API ÎÄµµ",
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

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
}
else
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
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
