using CodeGenerator.Generate;
using CodeGenerator.Test.Hepler;

namespace CodeGenerator.Test;

public class RestApiGenerateTest
{
    [Fact]
    public void Should_generate_rest_api_content()
    {
        var entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        var projectPath = PathHelper.GetProjectPath();
        var gen = new RestApiGenerate(entityPath, projectPath, projectPath,projectPath);

        var restInterface = gen.GetRestApiInterface();
        var restBase = gen.GetRestApiBase();
        var restContent = gen.GetRestApiContent();
        var globalUsings = gen.GetGlobalUsings();

        Assert.NotNull(globalUsings);
        Assert.NotNull(restBase);
        Assert.NotNull(restContent);
        Assert.NotNull(restInterface);

    }

}
