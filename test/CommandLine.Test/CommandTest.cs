using System.Threading.Tasks;

using CodeGenerator;
using CodeGenerator.Helper;
using CodeGenerator.Test.Hepler;

using Command.Share;
using Command.Share.Commands;

namespace CommandLine.Test;

public class CommandTest
{
    private static readonly string ProjectPath = @"E:\codes\DevCenter\src\";
    protected string EntityPath = ProjectPath + @"Core\Entities\ResourceAttributeDefine.cs";
    protected string DtoPath = ProjectPath + @"Share";
    protected string StorePath = ProjectPath + @"Http.Application";
    protected string ApiPath = ProjectPath + @"Http.API";


    private void SetEnv()
    {
        EntityPath = PathHelper.GetProjectFilePath(@"..\CodeGenerator.Test\Entity\Blog.cs"); ;
        DtoPath = PathHelper.GetProjectPath();
        StorePath = DtoPath;
        ApiPath = DtoPath;
    }

    [Fact]
    public async Task Shoud_generate_dto_filesAsync()
    {
        DtoCommand cmd = new(EntityPath, DtoPath);
        await cmd.RunAsync();
        string entityName = Path.GetFileNameWithoutExtension(new FileInfo(EntityPath).Name);
        string generateFile = Path.Combine(DtoPath, "Models", $"{entityName}Dtos", $"{entityName}UpdateDto.cs");
        string usingFile = Path.Combine(DtoPath, $"GlobalUsings.cs");
        Assert.True(File.Exists(generateFile));
        Assert.True(File.Exists(usingFile));
    }

    [Fact]
    public async Task Should_generate_store_filesAsync()
    {
        ManagerCommand cmd = new(EntityPath, StorePath, StorePath);
        await cmd.RunAsync(true);
        string storeInterfaceFile = Path.Combine(StorePath, "Interface", "IDataStore.cs");
        string userInterfaceFile = Path.Combine(StorePath, "Interface", "IUserContext.cs");
        string storeBaseFile = Path.Combine(StorePath, "DataStore", "DataStoreBase.cs");
        string storeFile = Path.Combine(StorePath, "DataStore", "BlogDataStore.cs");
        string userFile = Path.Combine(StorePath, "UserContext.cs");
        string serviceFile = Path.Combine(StorePath, "DataStore", "DataStoreExtensions.cs");
        string globalFile = Path.Combine(StorePath, GenConst.GLOBAL_USING_NAME);

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
        ApiCommand cmd = new(EntityPath, DtoPath, StorePath, ApiPath);
        await cmd.RunAsync();
        string apiInterfaceFile = Path.Combine(ApiPath, "Interface", GenConst.IRESTAPI_BASE_NAME);
        string apiBaseFile = Path.Combine(ApiPath, "Controllers", GenConst.RESTAPI_BASE_NAME);
        string apiFile = Path.Combine(ApiPath, "Controllers", "BlogController");
        string globalFile = Path.Combine(ApiPath, GenConst.GLOBAL_USING_NAME);

        Assert.True(File.Exists(apiInterfaceFile));
        Assert.True(File.Exists(apiBaseFile));
        Assert.True(File.Exists(apiFile));
        Assert.True(File.Exists(globalFile));
    }

    [Fact]
    public async Task Should_genearte_docAsync()
    {
        string url = "http://localhost:5002/swagger/v1/swagger.json";
        DocCommand cmd = new(url, "./");
        await cmd.RunAsync();
    }

    [Fact]
    public void Should_Config()
    {
        DirectoryInfo currentDir = new(ProjectPath);
        _ = AssemblyHelper.GetSlnFile(currentDir, currentDir.Root);

        System.Console.WriteLine("");
    }


    [Fact]
    public async Task Should_generate_ApiClientAsync()
    {
        string url = "http://localhost:5002/swagger/admin/swagger.json";
        string outputPath = @"d:\test";
        await CommandRunner.GenerateCSharpApiClientAsync(url, outputPath);
    }
}
