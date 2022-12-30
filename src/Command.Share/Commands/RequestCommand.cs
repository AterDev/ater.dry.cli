using Microsoft.OpenApi.Models;

namespace Command.Share.Commands;

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
        string openApiContent = "";
        if (DocUrl.StartsWith("http://") || DocUrl.StartsWith("https://"))
        {
            using HttpClient http = new();
            openApiContent = await http.GetStringAsync(DocUrl);
        }
        else
        {
            openApiContent = File.ReadAllText(DocUrl);
        }
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");
        ApiDocument = new OpenApiStringReader()
            .Read(openApiContent, out _);


        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();
        await GenerateRequestServicesAsync();
        Console.WriteLine("😀 Request services generate completed!" + Environment.NewLine);
    }


    public async Task GenerateCommonFilesAsync()
    {
        string content = RequestGenerate.GetBaseService(LibType);
        string dir = Path.Combine(OutputPath, "services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }

    public async Task GenerateRequestServicesAsync()
    {
        RequestGenerate ngGen = new(ApiDocument!)
        {
            LibType = LibType
        };
        // 获取请求服务并生成文件
        List<Core.Models.GenFileInfo> services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (Core.Models.GenFileInfo service in services)
        {
            string dir = Path.Combine(OutputPath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
        // 获取对应的ts模型类，生成文件
        Console.WriteLine(Instructions[1]);
        List<Core.Models.GenFileInfo> models = ngGen.GetTSInterfaces();
        foreach (Core.Models.GenFileInfo model in models)
        {
            string dir = Path.Combine(OutputPath, "models", model.Path.ToHyphen());
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
    }
}
