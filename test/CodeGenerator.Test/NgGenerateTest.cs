using CodeGenerator.Generate;

using Microsoft.OpenApi.Readers;

using System.IO;

namespace CodeGenerator.Test;

public class NgGenerateTest
{
    [Fact]
    public void should_generate_ts_models_from_openapiAsync()
    {
        var projectPath = PathHelper.GetProjectPath();
        var file = Path.Combine(projectPath,"Data/openapi.json");
        var ApiDocument = new OpenApiStringReader().Read(File.ReadAllText(file), out var context);
        var ngGen = new RequestGenearte(ApiDocument)
        {
            LibType = RequestLibType.Axios
        };
        // 获取对应的ts模型类，生成文件
        var models = ngGen.GetTSInterfaces();
        Console.WriteLine(models.Count);
    }

    [Fact]
    public void should_generate_ng_services()
    {
        var projectPath = PathHelper.GetProjectPath();
        var file = Path.Combine(projectPath,"Data/openapi.json");
        var openApiDoc = new OpenApiStringReader().Read(File.ReadAllText(file), out _);

        var serviceGen = new NgServiceGenerate(openApiDoc.Paths);
        var services = serviceGen.GetServices(openApiDoc.Tags);

        Assert.NotNull(services);
    }

    [Fact]
    public void should_generate_axios_services()
    {
        var projectPath = PathHelper.GetProjectPath();
        var file = Path.Combine(projectPath,"Data/openapi.json");
        var openApiDoc = new OpenApiStringReader().Read(File.ReadAllText(file), out _);

        var serviceGen = new RequestGenearte(openApiDoc)
        {
            LibType = RequestLibType.Axios
        };
        var services = serviceGen.GetServices(openApiDoc.Tags);

        Assert.NotNull(services);
    }


    [Fact]
    public void should_generate_ng_component()
    {
        var entityName = "Article";
        var dtoPath = @"D:\codes\DevPlatform\src\Share";
        var output=@"D:\codes\DevPlatform\src\Webapp\Admin";
        var gen = new NgPageGenerate(entityName,dtoPath,output);

        var dialog = NgPageGenerate.BuildConfirmDialog();
        var component = gen.BuildAddPage();
        Assert.Equal("add", component.Name);
        Assert.NotNull(dialog);
    }
}
