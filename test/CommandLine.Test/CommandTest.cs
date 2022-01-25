using CodeGenerator.Infrastructure;
using CodeGenerator.Test.Hepler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandLine.Test;

public class CommandTest
{
    protected string EntityPath = "";
    protected string DtoPath = "";
    protected string StorePath = "";
    protected string ApiPath = "";


    private void SetEnv()
    {
        EntityPath = PathHelper.GetProjectFilePath(@"..\CodeGenerator.Test\Entity\Blog.cs"); ;
        DtoPath = PathHelper.GetProjectPath();
        StorePath = DtoPath;
        ApiPath = DtoPath;
    }

    [Fact]
    public async Task Shoud_generate_filesAsync()
    {
        var cmd = new DtoCommand(EntityPath, DtoPath);
        await cmd.RunAsync();

        var entityName = Path.GetFileNameWithoutExtension(new FileInfo(EntityPath).Name);
        var generateFile = Path.Combine(DtoPath, "Models", $"{entityName}Dtos", $"{entityName}UpdateDto.cs");
        var usingFile = Path.Combine(DtoPath, "Models", $"GlobalUsing.cs");
        Assert.True(File.Exists(generateFile));
        Assert.True(File.Exists(usingFile));
    }

    [Fact]
    public async Task Should_generate_store_filesAsync()
    {
        var cmd = new StoreCommand(EntityPath, StorePath, StorePath);
        await cmd.RunAsync();
        var storeInterfaceFile = Path.Combine(StorePath, "Interface", "IDataStore.cs");
        var userInterfaceFile = Path.Combine(StorePath, "Interface", "IUserContext.cs");
        var storeBaseFile = Path.Combine(StorePath, "DataStore", "DataStoreBase.cs");
        var storeFile = Path.Combine(StorePath, "DataStore", "BlogDataStore.cs");
        var userFile = Path.Combine(StorePath, "UserContext.cs");
        var serviceFile = Path.Combine(StorePath, "DataStore", "DataStoreExtensions.cs");
        var globalFile = Path.Combine(StorePath, GenConst.GLOBAL_USING_NAME);

        Assert.True(File.Exists(storeInterfaceFile));
        Assert.True(File.Exists(userInterfaceFile));
        Assert.True(File.Exists(storeBaseFile));
        Assert.True(File.Exists(userFile));
        Assert.True(File.Exists(storeFile));
        Assert.True(File.Exists(serviceFile));
        Assert.True(File.Exists(globalFile));
    }

    [Fact]
    public async Task Should_generate_api_filesAsync()
    {
        var cmd = new ApiCommand(EntityPath, DtoPath, StorePath, ApiPath);
        await cmd.RunAsync();
        var apiInterfaceFile = Path.Combine(ApiPath, "Interface", GenConst.IRESTAPI_BASE_NAME);
        var apiBaseFile = Path.Combine(ApiPath, "Controllers", GenConst.RESTAPI_BASE_NAME);
        var apiFile = Path.Combine(ApiPath, "Controllers","BlogController");
        var globalFile = Path.Combine(ApiPath, GenConst.GLOBAL_USING_NAME);

        Assert.True(File.Exists(apiInterfaceFile));
        Assert.True(File.Exists(apiBaseFile));
        Assert.True(File.Exists(apiFile));
        Assert.True(File.Exists(globalFile));
    }
}
