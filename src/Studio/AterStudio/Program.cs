using System.Text;
using AterStudio.Worker;
using Http.API;

Console.OutputEncoding = Encoding.UTF8;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddDefaultWebServices();

WebApplication app = builder.Build();
app.UseDefaultWebServices();

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    await InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

