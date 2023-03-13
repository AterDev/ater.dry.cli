using Microsoft.OpenApi.Models;
namespace Command.Share.Commands;

/// <summary>
/// 请求生成命令
/// </summary>
public class RequestCommand : CommandBase
{
    /// <summary>
    /// swagger文档链接
    /// </summary>
    public string DocUrl { get; set; }
    /// <summary>
    /// 文档名称 swagger/{documentName}/swagger.json
    /// </summary>
    public string DocName { get; set; }

    public OpenApiDocument? ApiDocument { get; set; }

    public RequestLibType LibType { get; set; } = RequestLibType.NgHttp;

    public string OutputPath { get; set; }

    public RequestCommand(string docUrl, string output, RequestLibType libType)
    {
        DocUrl = docUrl;
        DocName = docUrl.Split('/').Reverse().Skip(1).First();

        // 兼容过去没有分组的生成
        if (DocName == "v1") DocName = string.Empty;
        OutputPath = Path.Combine(output, DocName);
        LibType = libType;

        Instructions.Add($"  🔹 generate ts interfaces.");
        Instructions.Add($"  🔹 generate request services.");
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

        // 获取对应的ts模型类，生成文件
        List<GenFileInfo> models = ngGen.GetTSInterfaces();
        foreach (GenFileInfo model in models)
        {
            string dir = Path.Combine(OutputPath, "models", model.Path.ToHyphen());
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }

        // 获取请求服务并生成文件
        List<GenFileInfo> services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (GenFileInfo service in services)
        {
            string dir = Path.Combine(OutputPath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }

    }
}
