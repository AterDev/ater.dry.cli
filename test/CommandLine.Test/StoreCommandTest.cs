using CodeGenerator.Generate;
using CodeGenerator.Test.Hepler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine.Test;

public class StoreCommandTest
{
    [Fact]
    public async Task Should_generate_store_filesAsync()
    {
        var entityPath = @"C:\self\cli\test\CodeGenerator.Test\Entity\Blog.cs";
        var projectPath = PathHelper.GetProjectPath();
        var cmd = new StoreCommand(entityPath, projectPath, projectPath);
        await cmd.RunAsync();

        var storeInterfaceFile = Path.Combine(projectPath, "Interface", "IDataStore.cs");
        var userInterfaceFile = Path.Combine(projectPath, "Interface", "IUserContext.cs");
        var storeBaseFile = Path.Combine(projectPath, "DataStore", "DataStoreBase.cs");
        var storeFile = Path.Combine(projectPath, "DataStore", "BlogDataStore.cs");
        var userFile = Path.Combine(projectPath, "UserContext.cs");
        var serviceFile = Path.Combine(projectPath, "DataStore", "DataStoreExtensions.cs");
        var globalFile = Path.Combine(projectPath, "GlobalUsings.cs");

        Assert.True(File.Exists(storeInterfaceFile));
        Assert.True(File.Exists(userInterfaceFile));
        Assert.True(File.Exists(storeBaseFile));
        Assert.True(File.Exists(userFile));
        Assert.True(File.Exists(storeFile));
        Assert.True(File.Exists(serviceFile));
        Assert.True(File.Exists(globalFile));

    }
}
