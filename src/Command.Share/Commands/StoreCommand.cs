using Core.Infrastructure;

namespace Command.Share.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class StoreCommand : CommandBase
{
    public string EntityPath { get; set; }
    public string StorePath { get; set; }
    public string DtoPath { get; set; }
    public ManagerGenerate CodeGen { get; set; }

    public StoreCommand(string entityPath, string dtoPath, string servicePath, string? contextName = null)
    {
        EntityPath = entityPath;
        StorePath = servicePath;
        DtoPath = dtoPath;
        CodeGen = new ManagerGenerate(entityPath, dtoPath, servicePath, contextName);
        string entityName = Path.GetFileNameWithoutExtension(entityPath);
        Instructions.Add($"  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} DataStore.");
        Instructions.Add($"  🔹 generate Manager files.");
        Instructions.Add($"  🔹 generate Manager test files.");
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
        await GenerateMangerTestAsync();

        Console.WriteLine(Instructions[4]);
        await GenerateServicesAsync();

        Console.WriteLine(Instructions[5]);
        await GenerateGlobalUsingsFilesAsync();

        Console.WriteLine("😀 Manager generate completed!" + Environment.NewLine);
    }

    /// <summary>
    /// 生成接口和实现类
    /// </summary>
    public async Task GenerateCommonFilesAsync()
    {
        // 生成Utils 扩展类
        DirectoryInfo? dir = new FileInfo(EntityPath).Directory;
        FileInfo? projectFile = AssemblyHelper.FindProjectFile(dir!, dir!.Root);
        if (projectFile != null)
        {
            string entityDir = Path.Combine(projectFile.Directory!.FullName, "Utils");
            string content = CodeGen.GetExtensions();
            await GenerateFileAsync(entityDir, GenConst.EXTIONSIONS_NAME, content);
        }

        // 目录
        string interfaceDir = Path.Combine(StorePath, "Interface");
        string implementDir = Path.Combine(StorePath, "Implement");

        // 文件
        string[] interfaceFiles = new string[] { "ICommandStore", "ICommandStoreExt", "IQueryStore", "IQueryStoreExt", "IDomainManager", "IUserContext" };


        string[] implementFiles = new string[] { "CommandStoreBase", "QueryStoreBase", "DomainManagerBase" };
        string userClass = CodeGen.GetUserContextClass();


        // 生成接口文件
        foreach (string name in interfaceFiles)
        {
            string content = CodeGen.GetInterfaceFile(name);
            await GenerateFileAsync(interfaceDir, $"{name}.cs", content, true);
        }
        // 生成实现文件
        foreach (string name in implementFiles)
        {
            string content = CodeGen.GetImplementFile(name);
            await GenerateFileAsync(implementDir, $"{name}.cs", content, true);
        }
        // 生成user上下文
        await GenerateFileAsync(implementDir, "UserContext.cs", userClass);

    }

    /// <summary>
    /// 生成manager
    /// </summary>
    public async Task GenerateMangerAsync()
    {
        string iManagerDir = Path.Combine(StorePath, "IManager");
        string managerDir = Path.Combine(StorePath, "Manager");
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);

        string interfaceContent = CodeGen.GetIManagerContent();
        string managerContent = CodeGen.GetManagerContent();
        // 生成接口
        await GenerateFileAsync(iManagerDir, $"I{entityName}Manager.cs", interfaceContent);
        // 生成manger
        await GenerateFileAsync(managerDir, $"{entityName}Manager.cs", managerContent);
    }


    public async Task GenerateMangerTestAsync()
    {
        string testProjectPath = Path.Combine(StorePath, "..", "..", "test", "Application.Test");
        if (Directory.Exists(testProjectPath))
        {
            string testDir = Path.Combine(testProjectPath, "Managers");
            string entityName = Path.GetFileNameWithoutExtension(EntityPath);
            if (Directory.Exists(testDir))
            {
                Directory.CreateDirectory(testDir);
            }
            string managerContent = CodeGen.GetManagerTestContent();
            await GenerateFileAsync(testDir, $"{entityName}ManagerTest.cs", managerContent);
        }
    }

    /// <summary>
    /// 生成全局依赖文件GlobalUsings.cs
    /// </summary>
    /// <returns></returns>
    public async Task GenerateGlobalUsingsFilesAsync()
    {
        List<string> globalUsings = CodeGen.GetGlobalUsings();
        string filePath = Path.Combine(StorePath, "GlobalUsings.cs");
        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
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
        string queryStoreDir = Path.Combine(StorePath, "QueryStore");
        string commandStoreDir = Path.Combine(StorePath, "CommandStore");
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        string queryStoreContent = CodeGen.GetStoreContent("Query");
        string commandStoreContent = CodeGen.GetStoreContent("Command");

        await GenerateFileAsync(queryStoreDir, $"{entityName}QueryStore.cs", queryStoreContent);
        await GenerateFileAsync(commandStoreDir, $"{entityName}CommandStore.cs", commandStoreContent);
    }
    /// <summary>
    /// 生成注入服务
    /// </summary>
    public async Task GenerateServicesAsync()
    {
        string implementDir = Path.Combine(StorePath, "Implement");
        string storeService = CodeGen.GetStoreService();
        string storeContext = CodeGen.GetDataStoreContext();

        // 生成仓储上下文
        await GenerateFileAsync(implementDir, "DataStoreContext.cs", storeContext, true);
        await GenerateFileAsync(implementDir, "StoreServicesExtensions.cs", storeService, true);


    }
}
