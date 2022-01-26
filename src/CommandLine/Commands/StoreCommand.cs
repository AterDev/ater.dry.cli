using CodeGenerator.Infrastructure;
using CodeGenerator.Infrastructure.Helper;
namespace Droplet.CommandLine.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class StoreCommand : CommandBase
{
    public string EntityPath { get; set; }
    public string StorePath { get; set; }
    public string DtoPath { get; set; }
    public DataStoreGenerate CodeGen { get; set; }

    public StoreCommand(string entityPath, string dtoPath, string servicePath, string? contextName = null)
    {
        EntityPath = entityPath;
        StorePath = servicePath;
        DtoPath = dtoPath;
        CodeGen = new DataStoreGenerate(entityPath, dtoPath, servicePath, contextName);
        var entityName = Path.GetFileNameWithoutExtension(entityPath);
        Instructions.Add("  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} DataStore.");
        Instructions.Add($"  🔹 update Globalusings files.");
        Instructions.Add($"  🔹 update Services inject files.");
    }

    /// <summary>
    /// 生成仓储
    /// </summary>
    public async Task RunAsync()
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine($"the {EntityPath} not exist");
            return;
        }
        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();
        Console.WriteLine(Instructions[1]);
        await GenerateStoreDataAsync();
        Console.WriteLine(Instructions[2]);
        await GenerateGlobalUsingsFilesAsync();
        Console.WriteLine(Instructions[3]);
        await GenerateServicesAsync();
        Console.WriteLine("😀 DataStroe generate completed!" + Environment.NewLine);
    }

    /// <summary>
    /// 生成接口和实现类
    /// </summary>
    public async Task GenerateCommonFilesAsync()
    {
        // 生成Utils 扩展类
        var dir = new FileInfo(EntityPath).Directory;
        var projectFile =  AssemblyHelper.FindProjectFile(dir!, dir!.Root);
        if (projectFile != null)
        {
            var entityDir = Path.Combine(projectFile.Directory!.FullName,"Utils" );
            var content = CodeGen.GetExtensions();
            await GenerateFileAsync(entityDir, GenConst.EXTIONSIONS_NAME, content);
        }

        var interfaceDir = Path.Combine(StorePath, "Interface");
        var storeDir = Path.Combine(StorePath, "DataStore");
        var storeInterface = CodeGen.GetStoreInterface();
        var userInterface = CodeGen.GetUserContextInterface();
        var userClass = CodeGen.GetUserContextClass();
        var storeBase = CodeGen.GetStoreBase();
        await GenerateFileAsync(interfaceDir, "IDataStore.cs", storeInterface);
        await GenerateFileAsync(interfaceDir, "IUserContext.cs", userInterface);
        await GenerateFileAsync(storeDir, "DataStoreBase.cs", storeBase);
        await GenerateFileAsync(StorePath, "UserContext.cs", userClass);
    }

    /// <summary>
    /// 生成全局依赖文件GlobalUsings.cs
    /// </summary>
    /// <returns></returns>
    public async Task GenerateGlobalUsingsFilesAsync()
    {
        var globalUsings = CodeGen.GetGlobalUsings();
        var filePath = Path.Combine(StorePath, "GlobalUsings.cs");
        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            globalUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();
            globalUsings.Insert(0, Environment.NewLine);
            if (globalUsings.Any())
                File.AppendAllLines(filePath, globalUsings);
        }
        else
        {
            await GenerateFileAsync(StorePath, "GlobalUsings.cs",
                string.Join(Environment.NewLine, globalUsings));
        }

    }
    /// <summary>
    /// 生成仓储
    /// </summary>
    public async Task GenerateStoreDataAsync()
    {
        var storeDir = Path.Combine(StorePath, "DataStore");
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        var storeContent = CodeGen.GetStoreContent();
        await GenerateFileAsync(storeDir, $"{entityName}DataStore.cs", storeContent);
    }
    /// <summary>
    /// 生成注入服务
    /// </summary>
    public async Task GenerateServicesAsync()
    {
        var storeDir = Path.Combine(StorePath, "DataStore");
        var storeService = CodeGen.GetStoreService();
        await GenerateFileAsync(storeDir, "DataStoreExtensions.cs", storeService, true);
    }
}
