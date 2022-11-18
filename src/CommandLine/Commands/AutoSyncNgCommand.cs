
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

        string swaggerPath = "./swagger.json";
        string openApiContent = File.ReadAllText(swaggerPath);
        ApiDocument = new OpenApiStringReader()
           .Read(openApiContent, out _);

        Console.WriteLine(Instructions[0]);
        await GenerateTsInterfacesAsync();
        await GenerateCommonFilesAsync();
        await GenerateNgServicesAsync();
        Console.WriteLine("😀 Ng services sync completed!" + Environment.NewLine);
        // 2 同步路由、页面
        Console.WriteLine(Instructions[1]);
        await GeneratePagesAsync();
        Console.WriteLine("😀 Ng view sync completed!" + Environment.NewLine);
    }

    /// <summary>
    /// 同步生成页面
    /// </summary>
    /// <returns></returns>
    public async Task GeneratePagesAsync()
    {
        // 获取所有实体，筛选出带有页面特性的类
        string entityDir = Path.Combine("..", ConfigOptions.EntityPath);
        if (!Directory.Exists(entityDir))
        {
            Console.WriteLine(entityDir + "不存在，跳过");
            return;

        }
        string[] files = Directory.GetFiles(entityDir, "*.cs", SearchOption.AllDirectories);
        List<FileInfo> fileInfos = new();
        // 筛选出只包含特性文本的实体
        foreach (string file in files)
        {
            string content = await File.ReadAllTextAsync(file);
            if (content.Contains("[NgPage("))
            {
                fileInfos.Add(new FileInfo(file));
            }
        }

        string ngPath = Path.Combine("ClientApp");
        string dtoPath = Path.Combine("..", ConfigOptions.DtoPath);
        ViewCommand cmd = new(dtoPath, ngPath);
        foreach (FileInfo entity in fileInfos)
        {
            EntityParseHelper entityParse = new(entity.FullName);
            entityParse.Parse();

            cmd.SetEntityPath(entity.FullName);
            cmd.Route = entityParse.NgRoute;
            cmd.ModuleName = entityParse.NgModuleName;
            cmd.EntityComment = entityParse.CommentContent;
            await cmd.RunAsync();
        }
        // 组模块
        await cmd.GenerateModuleRouteAsync();
        // 更新菜单 navigation.html
        await cmd.UpdateMenus();
    }

    public async Task GenerateTsInterfacesAsync()
    {
        // 获取对应的ts模型类，生成文件
        RequestGenearte ngGen = new(ApiDocument!);
        Console.WriteLine(Instructions[1]);
        List<GenFileInfo> models = ngGen.GetTSInterfaces();
        foreach (GenFileInfo model in models)
        {
            string dir = Path.Combine(SharePath, "models", model.Path.ToHyphen());
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
    }

    public async Task GenerateCommonFilesAsync()
    {
        string content = RequestGenearte.GetBaseService(RequestLibType.NgHttp);
        string dir = Path.Combine(SharePath, "services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }

    public async Task GenerateNgServicesAsync()
    {
        RequestGenearte ngGen = new(ApiDocument!);
        List<GenFileInfo> services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (GenFileInfo service in services)
        {
            string dir = Path.Combine(SharePath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
    }
}
