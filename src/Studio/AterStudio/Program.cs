using AterStudio;
using AterStudio.Worker;
using Mapster;

TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddDefaultComponents();
builder.AddDefaultWebServices();

WebApplication app = builder.Build();
app.UseDefaultWebServices();

using (app)
{
    IServiceScope scope = app.Services.CreateScope();
    InitDataTask.InitDataAsync(scope.ServiceProvider);
    app.Run();
}

