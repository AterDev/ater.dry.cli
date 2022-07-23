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
        Instructions.Add($"  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} DataStore.");
        Instructions.Add($"  🔹 generate Manger files.");
        Instructions.Add($"  🔹 generate Services inject files.");
        Instructions.Add($"  🔹 update Globalusings files.");
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
        await GenerateStoreFilesAsync();

        Console.WriteLine(Instructions[2]);
        await GenerateMangerAsync();

        Console.WriteLine(Instructions[3]);
        await GenerateServicesAsync();

        Console.WriteLine(Instructions[4]);
        await GenerateGlobalUsingsFilesAsync();

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
        var implementDir = Path.Combine(StorePath, "Implement");

        // 文件
        var interfaceFiles = new string[]{"ICommandStore","ICommandStoreExt","IQueryStore","IQueryStoreExt","IDomainManager","IUserContext"};


        var implementFiles = new string[]{"CommandStoreBase","QueryStoreBase"};
        var userClass = CodeGen.GetUserContextClass();


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
        await GenerateFileAsync(implementDir, "UserContext.cs", userClass);

    }

    /// <summary>
    /// 生成manager
    /// </summary>
    public async Task GenerateMangerAsync()
    {
        var iManagerDir = Path.Combine(StorePath, "IManager");
        var managerDir = Path.Combine(StorePath, "Manager");
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);

        var interfaceContent = CodeGen.GetIManagerContent();
        var managerContent = CodeGen.GetManagerContext();
        // 生成接口
        await GenerateFileAsync(iManagerDir, $"I{entityName}Manager.cs", interfaceContent);
        // 生成manger
        await GenerateFileAsync(managerDir, $"{entityName}Manager.cs", managerContent);
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
    public async Task GenerateStoreFilesAsync()
    {
        var queryStoreDir = Path.Combine(StorePath, "QueryStore");
        var commandStoreDir = Path.Combine(StorePath, "CommandStore");
        var entityName = Path.GetFileNameWithoutExtension(EntityPath);
        var queryStoreContent = CodeGen.GetStoreContent("Query");
        var commandStoreContent = CodeGen.GetStoreContent("Command");

        await GenerateFileAsync(queryStoreDir, $"{entityName}QueryStore.cs", queryStoreContent);
        await GenerateFileAsync(commandStoreDir, $"{entityName}CommandStore.cs", commandStoreContent);
    }
    /// <summary>
    /// 生成注入服务
    /// </summary>
    public async Task GenerateServicesAsync()
    {
        var implementDir = Path.Combine(StorePath, "Implement");
        var storeService = CodeGen.GetStoreService();
        var storeContext = CodeGen.GetDataStoreContext();

        // 生成仓储上下文
        await GenerateFileAsync(implementDir, "DataStoreContext.cs", storeContext);
        await GenerateFileAsync(implementDir, "StoreServicesExtensions.cs", storeService, true);


    }
}
