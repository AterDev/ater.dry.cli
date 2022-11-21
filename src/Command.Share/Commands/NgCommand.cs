using Microsoft.OpenApi.Models;

namespace Command.Share.Commands;

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

        ApiDocument = new OpenApiStringReader()
            .Read(openApiContent, out _);

        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();
        await GenerateNgServicesAsync();
        Console.WriteLine("😀 Ng services generate completed!" + Environment.NewLine);
    }


    public async Task GenerateCommonFilesAsync()
    {
        string content = NgServiceGenerate.GetBaseService();
        string dir = Path.Combine(SharePath, "services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }


    public async Task GenerateNgServicesAsync()
    {
        NgServiceGenerate ngGen = new(ApiDocument!.Paths);
        List<Core.Models.GenFileInfo> services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (Core.Models.GenFileInfo service in services)
        {
            string dir = Path.Combine(SharePath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
    }

}
