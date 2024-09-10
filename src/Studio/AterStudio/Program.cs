using System.Text;
using AterStudio;
using AterStudio.Worker;

Console.OutputEncoding = Encoding.UTF8;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddDefaultComponents();
builder.AddDefaultWebServices();

WebApplication app = builder.Build();
app.UseDefaultWebServices();

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

