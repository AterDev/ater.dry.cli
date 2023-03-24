using System;
using Microsoft.OpenApi.Models;
namespace Command.Share.Commands;
/// <summary>
/// 客户端请求生成
/// </summary>
public class ApiClientCommand : CommandBase
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

    public LanguageType LanguageType { get; set; } = LanguageType.CSharp;

    /// <summary>
    /// 输出目录
    /// </summary>
    public string OutputPath { get; set; }

    public ApiClientCommand(string docUrl, string output, LanguageType languageType)
    {
        DocUrl = docUrl;
        DocName = docUrl.Split('/').Reverse().Skip(1).First();

        // 兼容过去没有分组的生成
        if (DocName == "v1") DocName = string.Empty;
        OutputPath = output;
        LanguageType = languageType;

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
        Console.WriteLine("😀 Api Client generate completed!" + Environment.NewLine);
    }

    public async Task GenerateCommonFilesAsync()
    {
        string baseContent = CSHttpClientGenerate.GetBaseService();

        string globalUsingContent = CSHttpClientGenerate.GetGlobalUsing();

        string dir = Path.Combine(OutputPath, "Services");
        await GenerateFileAsync(dir, "BaseService.cs", baseContent, true);

        await GenerateFileAsync(OutputPath, "GlobalUsings.cs", globalUsingContent, false);

    }

    public async Task GenerateRequestServicesAsync()
    {
        var gen = new CSHttpClientGenerate(ApiDocument!);
        // 获取请求服务并生成文件
        List<GenFileInfo> services = gen.GetServices();
        foreach (GenFileInfo service in services)
        {
            string dir = Path.Combine(OutputPath, "Services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
        string clientContent = CSHttpClientGenerate.GetClient(services);
        await GenerateFileAsync(OutputPath, DocName.ToPascalCase() + "Client.cs", clientContent, true);
    }
}


public enum LanguageType
{
    CSharp
}