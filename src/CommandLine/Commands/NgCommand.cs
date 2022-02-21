using Microsoft.OpenApi.Models;
using System;

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
        Instructions.Add($"  🔹 generate Ts models.");
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
            .Read(openApiContent, out var context);

        Console.WriteLine(Instructions[0]);
        await GenerateTsModelsAsync();
        Console.WriteLine("😀 Ts models generate completed!" + Environment.NewLine);

        Console.WriteLine(Instructions[1]);
        await GenerateCommonFilesAsync();
        await GenerateNgServicesAsync();
        Console.WriteLine("😀 Ng services generate completed!" + Environment.NewLine);
    }


    public async Task GenerateCommonFilesAsync()
    {
        var ngGen = new NgServiceGenerate(ApiDocument!.Paths);
        var content = ngGen.GetBaseService();
        var dir = Path.Combine(SharePath,"services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }

    public async Task GenerateTsModelsAsync()
    {
        var schemas = ApiDocument!.Components.Schemas;
        var ngGen = new TSModelGenerate(schemas);
        var models = ngGen.GetInterfaces();
        foreach (var model in models)
        {
            var dir = Path.Combine(SharePath, "models",model.Path);
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
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
