using Microsoft.OpenApi.Models;

namespace Droplet.CommandLine.Commands;
public class DocCommand : CommandBase
{
    public string DocUrl { get; set; } = default!;
    public OpenApiDocument? ApiDocument { get; set; }

    public string OutputPath { get; set; }

    public DocCommand(string docUrl, string output)
    {
        DocUrl = docUrl;
        OutputPath = Path.Combine(output);
        Instructions.Add($"  🔹 generate docs.");
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
        await GenerateDocAsync(ApiDocument.Info.Title);
        Console.WriteLine("😀 markdown generate completed!" + Environment.NewLine);
    }

    public async Task GenerateDocAsync(string title)
    {
        IDictionary<string, OpenApiSchema> schemas = ApiDocument!.Components.Schemas;
        DocGenerate ngGen = new(schemas);
        if (ApiDocument!.Tags.Any())
        {
            ngGen.SetTags(ApiDocument!.Tags.ToList());
        }
        string content = ngGen.GetMarkdownContent();
        await GenerateFileAsync(OutputPath, title + ".md", content, true);

    }
}
