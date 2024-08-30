using System.IO;
using System.Text;

using CodeGenerator.Generate;
using CodeGenerator.Helper;

namespace CodeGenerator.Test;

public class StoreGenerateTest
{

    [Fact]
    public void Should_get_dbcontext_name()
    {
        string entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        string projectPath = PathHelper.GetProjectPath();
        ManagerGenerate gen = new(entityPath, projectPath, projectPath);
        string contextName = gen.GetContextName();
        Assert.Equal("TestDbContext", contextName);
    }

    [Fact]
    public void Should_generate_store_content()
    {
        string entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        string projectPath = PathHelper.GetProjectPath();

        //var storeInterface = gen.GetStoreInterface();
        //var storeBase = gen.GetStoreBase();
        //var storeContent = gen.GetStoreContent();
        //var globalUsings = gen.GetGlobalUsings();
        //var userInterface = gen.GetUserContextInterface();
        //var userContnet = gen.GetUserContextClass();

        //Assert.NotNull(globalUsings);
        //Assert.NotNull(storeBase);
        //Assert.NotNull(storeContent);
        //Assert.NotNull(storeInterface);
        //Assert.NotNull(userInterface);
        //Assert.NotNull(userContnet);

    }

    [Fact]
    public void Should_get_namespace()
    {
        string entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        FileInfo fileInfo = new(entityPath);
        FileInfo? projectFile = AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root);

        CompilationHelper compilationHelper = new(projectFile!.Directory!.FullName);
        string content = File.ReadAllText(fileInfo.FullName, new UTF8Encoding(false));
        compilationHelper.LoadContent(content);
        string? entityNamespace = compilationHelper.GetNamespace();

        Assert.NotNull(entityNamespace);


    }
}
