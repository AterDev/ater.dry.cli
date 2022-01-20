using CodeGenerator.Generate;
using CodeGenerator.Test.Hepler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Test;

public class StoreGenerateTest
{
    [Fact]
    public void Should_generate_store_files()
    {
        var entityPath = PathHelper.GetProjectFilePath("Entity/Blog.cs");
        var projectPath = PathHelper.GetProjectPath();

        var gen = new DataStoreGenerate(entityPath, projectPath, projectPath);

        var storeInterface = gen.GetStoreInterface();
        var storeBase = gen.GetStoreBase();
        var storeContent = gen.GetStoreContent();
        var storeService = gen.GetStoreService(new List<string> { "Blog" });
        var globalUsings = gen.GetGlobalUsings();

        Assert.NotNull(storeService);
        Assert.NotNull(globalUsings);
        Assert.NotNull(storeBase);
        Assert.NotNull(storeContent);
        Assert.NotNull(storeInterface);

    }

}
