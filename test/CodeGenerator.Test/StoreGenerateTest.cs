using CodeGenerator.Generate;
using CodeGenerator.Infrastructure.Helper;
using System.IO;
using System.Reflection;
using System.Text;

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
        var entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        var fileInfo = new FileInfo(entityPath);
        var projectFile = AssemblyHelper.FindProjectFile(fileInfo.Directory!, fileInfo.Directory!.Root);

        var compilationHelper = new CompilationHelper(projectFile.Directory!.FullName);
        var content = File.ReadAllText(fileInfo.FullName, Encoding.UTF8);
        compilationHelper.AddSyntaxTree(content);
        var  entityNamespace = compilationHelper.GetNamesapce();

        Assert.NotNull(entityNamespace);
        

    }
}
