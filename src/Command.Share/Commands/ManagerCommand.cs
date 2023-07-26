using Core.Infrastructure;

namespace Command.Share.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerCommand : CommandBase
{
    public string EntityPath { get; set; }
    public string ApplicationPath { get; set; }
    public string DtoPath { get; set; }
    public ManagerGenerate CodeGen { get; set; }
    /// <summary>
    /// 对应模块名
    /// </summary>
    public string? ModuleName { get; private set; }

    public ManagerCommand(string entityPath, string dtoPath, string applicationPath, string? contextName = null)
    {
        EntityPath = entityPath;
        ApplicationPath = applicationPath;
        DtoPath = dtoPath;
        CodeGen = new ManagerGenerate(entityPath, dtoPath, applicationPath, contextName);
        string entityName = Path.GetFileNameWithoutExtension(entityPath);
        Instructions.Add($"  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} DataStore.");
        Instructions.Add($"  🔹 generate Manager files.");
        Instructions.Add($"  🔹 generate Manager test files.");
        Instructions.Add($"  🔹 generate DataStoreContext files.");
        Instructions.Add($"  🔹 update Globalusings files.");
    }

    /// <summary>
    /// 生成仓储
    /// </summary>
    public async Task RunAsync(bool force)
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine($"the {EntityPath} not exist");
            return;
        }
        try
        {
            // 是否为模块
            var compilation = new CompilationHelper(ApplicationPath, "Entity");
            var content = File.ReadAllText(EntityPath);
            compilation.AddSyntaxTree(content);
            var attributes = compilation.GetClassAttribution("Module");
            if (attributes != null && attributes.Any())
            {
                ModuleName = attributes.First().ArgumentList!.Arguments[0].ToString().Trim('"');
            }
            // 生成到模块项目中
            if (!string.IsNullOrWhiteSpace(ModuleName))
            {
                ApplicationPath = Path.Combine(ApplicationPath, "..", "Modules", ModuleName);
                if (!Directory.Exists(ApplicationPath))
                {
                    Console.WriteLine($"⚠️ module {ModuleName} not exist, please create first!");
                    return;
                }

                Console.WriteLine(Instructions[2]);
                await GenerateMangerAsync(force);
                Console.WriteLine(Instructions[5]);
                await GenerateGlobalUsingsFilesAsync();
            }
            else
            {
                Console.WriteLine(Instructions[0]);
                await GenerateCommonFilesAsync(force);
                Console.WriteLine(Instructions[1]);
                await GenerateStoreFilesAsync();

                Console.WriteLine(Instructions[2]);
                await GenerateMangerAsync(force);

                Console.WriteLine(Instructions[3]);
                await GenerateMangerTestAsync(force);

                Console.WriteLine(Instructions[4]);
                await GetDataStoreContextAsync();
                await GenerateDIExtensionsAsync();

                Console.WriteLine(Instructions[5]);
                await GenerateGlobalUsingsFilesAsync();
            }

            Console.WriteLine("😀 Manager generate completed!" + Environment.NewLine);
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message + ex.StackTrace);
        }
    }


    /// <summary>
    /// 生成接口和实现类
    /// </summary>
    public async Task GenerateCommonFilesAsync(bool isCover = false)
    {
        // 目录
        string implementDir = Path.Combine(ApplicationPath, "Implement");

        // 文件
        string[] implementFiles = new string[] { "CommandStoreBase", "QueryStoreBase", "ManagerBase", "DomainManagerBase" };

        string userClass = CodeGen.GetUserContextClass();
        string content;
        // 生成实现文件
        foreach (string name in implementFiles)
        {
            content = CodeGen.GetImplementFile(name);
            content = content.Replace("${IdType}", Config.IdType);
            isCover = name != "DomainManagerBase" && isCover;
            await GenerateFileAsync(implementDir, $"{name}.cs", content, isCover);
        }

        content = CodeGen.GetInterfaceFile("IDomainManager");
        await GenerateFileAsync(Path.Combine(ApplicationPath, "IManager"), "IDomainManager.cs", content, true);

        content = CodeGen.GetInterfaceFile("IUserContext");
        await GenerateFileAsync(ApplicationPath, "IUserContext.cs", content);
        // 生成user上下文
        await GenerateFileAsync(implementDir, "UserContext.cs", userClass);

    }

    /// <summary>
    /// 生成manager
    /// </summary>
    public async Task GenerateMangerAsync(bool force)
    {
        string iManagerDir = Path.Combine(ApplicationPath, "IManager");
        string managerDir = Path.Combine(ApplicationPath, "Manager");
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);

        string interfaceContent = CodeGen.GetIManagerContent();
        string managerContent = CodeGen.GetManagerContent();

        // 如果文件已经存在，并且没有选择覆盖，并且符合更新要求，则进行更新
        string iManagerPath = Path.Combine(iManagerDir, $"I{entityName}Manager.cs");
        if (!force
            && File.Exists(iManagerPath)
            && AssemblyHelper.NeedUpdate(Const.Version))
        {
        }
        else
        {
            // 生成接口
            await GenerateFileAsync(iManagerDir, $"I{entityName}Manager.cs", interfaceContent, force);
        }

        // 生成manger
        await GenerateFileAsync(managerDir, $"{entityName}Manager.cs", managerContent, force);
    }

    public async Task GenerateMangerTestAsync(bool force)
    {
        string testProjectPath = Path.Combine(ApplicationPath, "..", "..", "test", "Application.Test");
        if (Directory.Exists(testProjectPath))
        {
            string testDir = Path.Combine(testProjectPath, "Managers");
            string entityName = Path.GetFileNameWithoutExtension(EntityPath);
            if (Directory.Exists(testDir))
            {
                _ = Directory.CreateDirectory(testDir);
            }
            string managerContent = CodeGen.GetManagerTestContent();
            await GenerateFileAsync(testDir, $"{entityName}ManagerTest.cs", managerContent, force);
        }
    }

    /// <summary>
    /// 生成全局依赖文件GlobalUsings.cs
    /// </summary>
    /// <returns></returns>
    public async Task GenerateGlobalUsingsFilesAsync()
    {
        List<string> globalUsings = CodeGen.GetGlobalUsings();
        string filePath = Path.Combine(ApplicationPath, "GlobalUsings.cs");
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
            await GenerateFileAsync(ApplicationPath, "GlobalUsings.cs",
                string.Join(Environment.NewLine, globalUsings));
        }
    }

    /// <summary>
    /// 生成仓储
    /// </summary>
    public async Task GenerateStoreFilesAsync()
    {
        string queryStoreDir = Path.Combine(ApplicationPath, "QueryStore");
        string commandStoreDir = Path.Combine(ApplicationPath, "CommandStore");
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);
        string queryStoreContent = CodeGen.GetStoreContent("Query");
        string commandStoreContent = CodeGen.GetStoreContent("Command");

        await GenerateFileAsync(queryStoreDir, $"{entityName}QueryStore.cs", queryStoreContent);
        await GenerateFileAsync(commandStoreDir, $"{entityName}CommandStore.cs", commandStoreContent);
    }

    /// <summary>
    /// 生成注入服务
    /// </summary>
    public async Task GetDataStoreContextAsync()
    {
        string storeContext = ManagerGenerate.GetDataStoreContext(ApplicationPath, "Application");
        // 生成仓储上下文
        await GenerateFileAsync(ApplicationPath, "DataStoreContext.cs", storeContext, true);
    }

    public async Task GenerateDIExtensionsAsync()
    {
        var content = ManagerGenerate.GetManagerDIExtensions(ApplicationPath, "Application");
        await GenerateFileAsync(ApplicationPath, "ManagerServiceCollectionExtensions.cs", content, true);
    }
}
