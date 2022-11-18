using CodeGenerator.Generate;

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

}
