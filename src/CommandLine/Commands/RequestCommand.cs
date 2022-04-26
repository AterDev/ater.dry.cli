using Microsoft.OpenApi.Models;

namespace Droplet.CommandLine.Commands;

public class RequestCommand : CommandBase
{
    public string DocUrl { get; set; } = default!;
    public OpenApiDocument? ApiDocument { get; set; }

    public RequestLibType LibType { get; set; } = RequestLibType.AngularHttpClient;

    public string OutputPath { get; set; }

    public RequestCommand(string docUrl, string output, RequestLibType libType)
    {
        DocUrl = docUrl;
        OutputPath = Path.Combine(output);
        LibType = libType;

        Instructions.Add($"  🔹 generate ts models.");
        Instructions.Add($"  🔹 generate request services.");
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
        await GenerateTsModelsAsync();
        Console.WriteLine("😀 Typescript models generate completed!" + Environment.NewLine);

        Console.WriteLine(Instructions[1]);
        await GenerateCommonFilesAsync();
        await GenerateNgServicesAsync();
        Console.WriteLine("😀 Request services generate completed!" + Environment.NewLine);
    }


    public async Task GenerateCommonFilesAsync()
    {
        var content = RequestGenearte.GetBaseService(LibType);
        var dir = Path.Combine(OutputPath,"services");
        await GenerateFileAsync(dir, "base.service.ts", content, false);
    }

    public async Task GenerateTsModelsAsync()
    {
        var schemas = ApiDocument!.Components.Schemas;
        var ngGen = new TSModelGenerate(schemas);
        if (ApiDocument!.Tags.Any())
        {
            ngGen.SetTags(ApiDocument!.Tags.ToList());
        }
        var models = ngGen.GetInterfaces();
        foreach (var model in models)
        {
            var dir = Path.Combine(OutputPath, "models", model.Path);
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
    }
    public async Task GenerateNgServicesAsync()
    {
        var ngGen = new RequestGenearte(ApiDocument!)
        {
            LibType = LibType
        };
        var services = ngGen.GetServices(ApiDocument!.Tags);
        foreach (var service in services)
        {
            var dir = Path.Combine(OutputPath, "services");
            await GenerateFileAsync(dir, service.Name, service.Content, true);
        }
    }

}
