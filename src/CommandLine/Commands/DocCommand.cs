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
        await GenerateDocAsync();
        Console.WriteLine("😀 markdown generate completed!" + Environment.NewLine);
    }

    public async Task GenerateDocAsync()
    {
        var schemas = ApiDocument!.Components.Schemas;
        var ngGen = new DocGenerate(schemas);
        if (ApiDocument!.Tags.Any())
        {
            ngGen.SetTags(ApiDocument!.Tags.ToList());
        }
        var content = ngGen.GetMarkdownContent();
        await GenerateFileAsync(OutputPath, "api doc.md", content, true);

    }
}
