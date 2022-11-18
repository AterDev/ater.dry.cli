using System.IO;
using CodeGenerator.Generate;
using Microsoft.OpenApi.Readers;

namespace CodeGenerator.Test;

public class NgGenerateTest
{
    [Fact]
    public void should_generate_ts_models_from_openapiAsync()
    {
        string projectPath = PathHelper.GetProjectPath();
        string file = Path.Combine(projectPath, "Data/openapi.json");
        Microsoft.OpenApi.Models.OpenApiDocument ApiDocument = new OpenApiStringReader().Read(File.ReadAllText(file), out _);
        RequestGenearte ngGen = new(ApiDocument)
        {
            LibType = RequestLibType.Axios
        };
        // 获取对应的ts模型类，生成文件
        List<Core.Models.GenFileInfo> models = ngGen.GetTSInterfaces();
        Console.WriteLine(models.Count);
    }

    [Fact]
    public void should_generate_ng_services()
    {
        string projectPath = PathHelper.GetProjectPath();
        string file = Path.Combine(projectPath, "Data/openapi.json");
        Microsoft.OpenApi.Models.OpenApiDocument openApiDoc = new OpenApiStringReader().Read(File.ReadAllText(file), out _);

        NgServiceGenerate serviceGen = new(openApiDoc.Paths);
        List<Core.Models.GenFileInfo> services = serviceGen.GetServices(openApiDoc.Tags);

        Assert.NotNull(services);
    }

    [Fact]
    public void should_generate_axios_services()
    {
        string projectPath = PathHelper.GetProjectPath();
        string file = Path.Combine(projectPath, "Data/openapi.json");
        Microsoft.OpenApi.Models.OpenApiDocument openApiDoc = new OpenApiStringReader().Read(File.ReadAllText(file), out _);

        RequestGenearte serviceGen = new(openApiDoc)
        {
            LibType = RequestLibType.Axios
        };
        List<Core.Models.GenFileInfo> services = serviceGen.GetServices(openApiDoc.Tags);

        Assert.NotNull(services);
    }


    [Fact]
    public void should_generate_ng_component()
    {
        string entityName = "BlogCatalog";
        string dtoPath = @"D:\codes\DevCenter\src\Share";
        string output = @"D:\codes\DevCenter\src\Webapp\Admin";
        NgPageGenerate gen = new(entityName, dtoPath, output);

        Core.Models.NgComponentInfo dialog = NgPageGenerate.BuildConfirmDialog();
        Core.Models.NgComponentInfo component = gen.BuildAddPage();
        Assert.Equal("add", component.Name);
        Assert.NotNull(dialog);
    }
}
