using CodeGenerator.Generate;

namespace CodeGenerator.Test;

public class StoreGenerateTest
{

    [Fact]
    public void Should_get_dbcontext_name()
    {
        var entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        var projectPath = PathHelper.GetProjectPath();
        var gen = new DataStoreGenerate(entityPath, projectPath, projectPath);
        var contextName = gen.GetContextName();
        Assert.Equal("TestDbContext", contextName);
    }

    [Fact]
    public void Should_generate_store_content()
    {
        var entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        var projectPath = PathHelper.GetProjectPath();
        var gen = new DataStoreGenerate(entityPath, projectPath, projectPath);

        var storeInterface = gen.GetStoreInterface();
        var storeBase = gen.GetStoreBase();
        var storeContent = gen.GetStoreContent();
        var globalUsings = gen.GetGlobalUsings();
        var userInterface = gen.GetUserContextInterface();
        var userContnet = gen.GetUserContextClass();

        Assert.NotNull(globalUsings);
        Assert.NotNull(storeBase);
        Assert.NotNull(storeContent);
        Assert.NotNull(storeInterface);
        Assert.NotNull(userInterface);
        Assert.NotNull(userContnet);

    }

}
