using Microsoft.OpenApi.Models;

namespace Droplet.CommandLine.Commands;
public class TSCommand : CommandBase
{
    public string DocUrl { get; set; } = default!;
    public OpenApiDocument? ApiDocument { get; set; }

    public string SharePath { get; set; }

    public TSCommand(string docUrl, string output)
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
            .Read(openApiContent, out _);

        Console.WriteLine(Instructions[0]);
        await GenerateTsModelsAsync();
        Console.WriteLine("😀 Ts models generate completed!" + Environment.NewLine);
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
            var dir = Path.Combine(SharePath, "models", model.Path);
            await GenerateFileAsync(dir, model.Name, model.Content, true);
        }
    }
}
