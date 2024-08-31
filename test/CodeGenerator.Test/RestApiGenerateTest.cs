using System.Net.Http;
using System.Threading.Tasks;

using CodeGenerator.Generate;

using Microsoft.OpenApi.Readers;

namespace CodeGenerator.Test;

public class RestApiGenerateTest
{
    [Fact]
    public void Should_generate_rest_api_content()
    {
        string entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        string projectPath = PathHelper.GetProjectPath();
        RestApiGenerate gen = new(entityPath, projectPath, projectPath, projectPath);

        string restInterface = gen.GetRestApiInterface();
        string restBase = gen.GetRestApiBase();
        string restContent = gen.GetRestApiContent();
        List<string> globalUsings = gen.GetGlobalUsings();

        Assert.NotNull(globalUsings);
        Assert.NotNull(restBase);
        Assert.NotNull(restContent);
        Assert.NotNull(restInterface);

    }

    /// <summary>
    /// csharp 请求client
    /// </summary>
    [Fact]
    public async Task Should_generate_csharpe_requestAsync()
    {
        string url = "http://localhost:5002/swagger/client/swagger.json";
        using HttpClient http = new();
        string openApiContent = await http.GetStringAsync(url);
        // 过滤特殊符号
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        Microsoft.OpenApi.Models.OpenApiDocument ApiDocument = new OpenApiStringReader()
           .Read(openApiContent, out _);
        CSHttpClientGenerate gen = new(ApiDocument);
        gen.GetServices("test");

    }

}
