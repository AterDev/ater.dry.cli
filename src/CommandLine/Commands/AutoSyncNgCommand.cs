using Microsoft.OpenApi.Models;

namespace Droplet.CommandLine.Commands;
public class AutoSyncNgCommand : CommandBase
{
    public ConfigOptions ConfigOptions { get; init; }
    public OpenApiDocument? ApiDocument { get; set; }
    public string SharePath { get; set; }
    public AutoSyncNgCommand()
    {
        ConfigOptions = ConfigCommand.ReadConfigFile()!;
        SharePath = Path.Combine("ClientApp", "src", "app", "share");
    }

    public async Task RunAsync()
    {
        // 1 自动同步ts类型和请求服务
        Instructions.Add($"  🔹 sync ng services.");
        Instructions.Add($"  🔹 sync ng pages.");

        var swaggerPath = "./swagger.json";
        var openApiContent = File.ReadAllText(swaggerPath);
        ApiDocument = new OpenApiStringReader()
           .Read(openApiContent, out _);

        Console.WriteLine(Instructions[0]);
        await GenerateTsInterfacesAsync();
        await GenerateCommonFilesAsync();
        await GenerateNgServicesAsync();
        Console.WriteLine("😀 Ng services generate completed!" + Environment.NewLine);
        // 2 同步路由、页面

    }
    public async Task GenerateTsInterfacesAsync()
    {
        // 获取对应的ts模型类，生成文件
        var ngGen = new RequestGenearte(ApiDocument!);
        Console.WriteLine(Instructions[1]);
        var models = ngGen.GetTSInterfaces();
        foreach (var model in models)
        {
            var dir = Path.Combine(SharePath, "models", model.Path.ToHyphen());
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
    }

    public async Task GenerateCommonFilesAsync()
    {
        var content = RequestGenearte.GetBaseService(RequestLibType.NgHttp);
        var dir = Path.Combine(SharePath,"services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }

    public async Task GenerateNgServicesAsync()
    {
        var ngGen = new RequestGenearte(ApiDocument!);
        var services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (var service in services)
        {
            var dir = Path.Combine(SharePath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
    }
}
