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
        var url = "http://localhost:5002/swagger/client/swagger.json";
        using HttpClient http = new();
        var openApiContent = await http.GetStringAsync(url);
        // 过滤特殊符号
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        var ApiDocument = new OpenApiStringReader()
           .Read(openApiContent, out _);
        var gen = new CSHttpClientGenerate(ApiDocument);
        gen.GetServices();

    }

}
