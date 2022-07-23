using CodeGenerator.Infrastructure;
using CodeGenerator.Infrastructure.Helper;
using System.Security.Cryptography.X509Certificates;

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
            var entityDir = Path.Combine(projectFile.Directory!.FullName, "Utils");
            var content = CodeGen.GetExtensions();
            await GenerateFileAsync(entityDir, GenConst.EXTIONSIONS_NAME, content);
        }

        // 目录
        var interfaceDir = Path.Combine(StorePath, "Interface");
        var iManagerDir = Path.Combine(StorePath, "IManager");
        var managerDir = Path.Combine(StorePath, "Manager");
        var implementDir = Path.Combine(StorePath, "Implement");
        var storeDir = Path.Combine(StorePath, "DataStore");

        // 文件
        var interfaceFiles = new string[]{"IDataStoreCommand","IDataStoreCommandExt","IDataStoreQuery","IDataStoreQueryExt","IDomainManager","IUserContext"};


        var implementFiles = new string[]{"CommandStoreBase","QueryStoreBase"};
        var userClass = CodeGen.GetUserContextClass();
        var storeContext = CodeGen.GetDataStoreContext();

        // 生成接口文件
        foreach (var name in interfaceFiles)
        {
            var content = CodeGen.GetInterfaceFile(name);
            await GenerateFileAsync(interfaceDir, $"{name}.cs", content);
        }
        // 生成实现文件
        foreach (var name in implementFiles)
        {
            var content = CodeGen.GetImplementFile(name);
            await GenerateFileAsync(implementDir, $"{name}.cs", content);
        }
        // 生成user上下文
        await GenerateFileAsync(StorePath, "UserContext.cs", userClass);
        // 生成仓储上下文
        await GenerateFileAsync(implementDir, "DataStoreContext.cs", storeContext);
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
            if (globalUsings.Any())
            {
                globalUsings.Insert(0, Environment.NewLine);
                File.AppendAllLines(filePath, globalUsings);

            }
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
        var queryStoreContent = CodeGen.GetStoreContent("Query");
        var commandStoreContent = CodeGen.GetStoreContent("Command");

        await GenerateFileAsync(storeDir, $"{entityName}QueryStore.cs", queryStoreContent);
        await GenerateFileAsync(storeDir, $"{entityName}CommandStore.cs", commandStoreContent);
    }
    /// <summary>
    /// 生成注入服务
    /// </summary>
    public async Task GenerateServicesAsync()
    {
        var storeDir = Path.Combine(StorePath, "Implement");
        var storeService = CodeGen.GetStoreService();
        await GenerateFileAsync(storeDir, "StoreServicesExtensions.cs", storeService, true);
    }
}
