using Microsoft.OpenApi.Models;

namespace Droplet.CommandLine.Commands;

public class NgCommand : CommandBase
{
    public string DocUrl { get; set; } = default!;
    public OpenApiDocument? ApiDocument { get; set; }

    public string SharePath { get; set; }

    public NgCommand(string docUrl, string output)
    {
        DocUrl = docUrl;
        SharePath = Path.Combine(output, "src", "app", "share");
        Instructions.Add($"  🔹 generate ng services.");
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
        await GenerateNgServicesAsync();
        Console.WriteLine("😀 Ng services generate completed!" + Environment.NewLine);
    }


    public async Task GenerateCommonFilesAsync()
    {
        var content = NgServiceGenerate.GetBaseService();
        var dir = Path.Combine(SharePath,"services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }


    public async Task GenerateNgServicesAsync()
    {
        var ngGen = new NgServiceGenerate(ApiDocument!.Paths);
        var services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (var service in services)
        {
            var dir = Path.Combine(SharePath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
    }

}
