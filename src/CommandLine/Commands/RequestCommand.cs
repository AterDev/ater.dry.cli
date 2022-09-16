using Microsoft.OpenApi.Models;

namespace Droplet.CommandLine.Commands;

public class RequestCommand : CommandBase
{
    public string DocUrl { get; set; } = default!;
    public OpenApiDocument? ApiDocument { get; set; }

    public RequestLibType LibType { get; set; } = RequestLibType.NgHttp;

    public string OutputPath { get; set; }

    public RequestCommand(string docUrl, string output, RequestLibType libType)
    {
        DocUrl = docUrl;
        OutputPath = Path.Combine(output);
        LibType = libType;

        Instructions.Add($"  🔹 generate request services.");
        Instructions.Add($"  🔹 generate ts interfaces.");
    }

    public async Task RunAsync()
    {
        var openApiContent = "";
        if (DocUrl.StartsWith("http://") || DocUrl.StartsWith("https://"))
        {
            using var http = new HttpClient();
            openApiContent = await http.GetStringAsync(DocUrl);
        }
        else
        {
            openApiContent = File.ReadAllText(DocUrl);
        }
        ApiDocument = new OpenApiStringReader()
            .Read(openApiContent, out _);

        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();
        await GenerateRequestServicesAsync();
        Console.WriteLine("😀 Request services generate completed!" + Environment.NewLine);
    }


    public async Task GenerateCommonFilesAsync()
    {
        var content = RequestGenearte.GetBaseService(LibType);
        var dir = Path.Combine(OutputPath,"services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }

    public async Task GenerateRequestServicesAsync()
    {
        var ngGen = new RequestGenearte(ApiDocument!)
        {
            LibType = LibType
        };
        // 获取请求服务并生成文件
        var services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (var service in services)
        {
            var dir = Path.Combine(OutputPath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
        // 获取对应的ts模型类，生成文件
        Console.WriteLine(Instructions[1]);
        var models = ngGen.GetTSInterfaces();
        foreach (var model in models)
        {
            var dir = Path.Combine(OutputPath, "models", model.Path.ToHyphen());
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
    }
}
