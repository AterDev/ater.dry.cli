using CodeGenerator.Generate;
using CodeGenerator.Test.Base;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGenerator.Test;

public class NgGenerateTest
{

    [Fact]
    public void should_generate_ts_models_from_openapi()
    {
        var projectPath = PathHelper.GetProjectPath();
        var file = Path.Combine(projectPath,"Data/openapi.json");
        var openApiDoc = new OpenApiStringReader().Read(File.ReadAllText(file), out var context);
        // 所有类型
        var schemas = openApiDoc.Components.Schemas;
        var tsGen = new TSModelGenerate(schemas);
        var files = tsGen.GetInterfaces();

        var updateDto = files.SingleOrDefault(f => f.Name == "article-catalog-update-dto.model.ts");
        Assert.NotNull(updateDto);
        Assert.Equal("article-catalog", updateDto!.Path);

        var statusDto = files.SingleOrDefault(f => f.Name == "status.model.ts");
        Assert.NotNull(statusDto);
        Assert.Equal("enum", statusDto!.Path);

        var pageDto = files.SingleOrDefault(f => f.Name == "page-result-of-article-catalog-item-dto.model.ts");
        Assert.NotNull(pageDto);
        Assert.Equal("article-catalog", pageDto!.Path);

        var userDto = files.SingleOrDefault(f => f.Name == "user.model.ts");
        Assert.NotNull(userDto);
        Assert.Equal("user", userDto!.Path);
    }

    [Fact]
    public void should_generate_ng_services()
    {
        var projectPath = PathHelper.GetProjectPath();
        var file = Path.Combine(projectPath,"Data/openapi.json");
        var openApiDoc = new OpenApiStringReader().Read(File.ReadAllText(file), out var context);

        var serviceGen = new NgServiceGenerate(openApiDoc.Paths);
        var services = serviceGen.GetServices(openApiDoc.Tags);

        Assert.NotNull(services);
    }
}
