using Core.Infrastructure;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PluralizeService.Core;

namespace Command.Share.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerCommand : CommandBase
{
    public string EntityFilePath { get; }
    public string ApplicationPath { get; private set; }
    public string SharePath { get; }
    public string StorePath { get; }
    public string SolutionPath { get; }
    public ManagerGenerate CodeGen { get; set; }
    /// <summary>
    /// 对应模块名
    /// </summary>
    public string? ModuleName { get; private set; }

    public ManagerCommand(string entityFilePath, string solutionPath)
    {
        SolutionPath = solutionPath;
        EntityFilePath = entityFilePath;
        ApplicationPath = Path.Combine(solutionPath, Config.ApplicationPath);
        SharePath = Path.Combine(solutionPath, Config.SharePath);
        StorePath = Path.Combine(solutionPath, Config.EntityFrameworkPath);

        CodeGen = new ManagerGenerate(EntityFilePath, SharePath, ApplicationPath);
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        Instructions.Add($"  🔹 generate interface & base class.");
        Instructions.Add($"  🔹 generate {entityName} DataStore.");
        Instructions.Add($"  🔹 generate Manager files.");
        Instructions.Add($"  🔹 generate Manager test files.");
        Instructions.Add($"  🔹 generate DataStoreContext files.");
        Instructions.Add($"  🔹 update Globalusings files.");
    }

    public ManagerCommand(string entityFilePath, string dtoPath, string applicationPath)
    {
        EntityFilePath = entityFilePath;
        ApplicationPath = applicationPath;
        SharePath = dtoPath;
        var currentDir = new DirectoryInfo(applicationPath);
        var solutionFile = AssemblyHelper.GetSlnFile(currentDir, currentDir.Root)
            ?? throw new Exception("not found solution file");

        SolutionPath = solutionFile.DirectoryName!;
        StorePath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);
        CodeGen = new ManagerGenerate(entityFilePath, dtoPath, applicationPath);
        string entityName = Path.GetFileNameWithoutExtension(entityFilePath);
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
        if (!File.Exists(EntityFilePath))
        {
            Console.WriteLine($"the {EntityFilePath} not exist");
            return;
        }
        try
        {
            AddToDbContext();
            // 是否为模块
            var compilation = new CompilationHelper(ApplicationPath, "Entity");
            var content = File.ReadAllText(EntityFilePath);
            compilation.AddSyntaxTree(content);
            var attributes = compilation.GetClassAttribution("Module");
            if (attributes != null && attributes.Any())
            {
                var argument = attributes.First().ArgumentList!.Arguments[0];
                if (argument.Expression is LiteralExpressionSyntax literal)
                {
                    ModuleName = literal.Token.ValueText;
                }
                else if (argument.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    ModuleName = memberAccess.Name.Identifier.ValueText;
                }
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
            if (name is "CommandStoreBase" or "QueryStoreBase")
            {
                var path = Path.Combine(StorePath, name.Replace("Base", ""));
                await GenerateFileAsync(path, $"{name}.cs", content, isCover);
            }
            else
            {
                await GenerateFileAsync(implementDir, $"{name}.cs", content, isCover);
            }
        }

        content = CodeGen.GetInterfaceFile("IDomainManager");
        await GenerateFileAsync(Path.Combine(ApplicationPath, "IManager"), "IDomainManager.cs", content, true);

        content = CodeGen.GetInterfaceFile("IUserContext");
        await GenerateFileAsync(ApplicationPath, "IUserContext.cs", content);
        // 生成user上下文
        await GenerateFileAsync(implementDir, "UserContext.cs", userClass);

    }

    /// <summary>
    /// 添加实体到数据库上下文
    /// </summary>
    /// <returns></returns>
    public void AddToDbContext()
    {
        var databasePath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);
        Console.WriteLine("🚀 update ContextBase DbSet");
        var dbContextFile = Path.Combine(databasePath, "ContextBase.cs");
        var dbContextContent = File.ReadAllText(dbContextFile);

        var compilation = new CompilationHelper(databasePath);
        compilation.AddSyntaxTree(dbContextContent);

        var entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        var plural = PluralizationProvider.Pluralize(entityName);
        var propertyString = $@"public DbSet<{entityName}> {plural} {{ get; set; }}";
        if (!compilation.PropertyExist(plural))
        {
            Console.WriteLine($"  ℹ️ add new property {plural} ➡️ ContextBase");
            compilation.AddClassProperty(propertyString);
        }
        dbContextContent = compilation.SyntaxRoot!.ToFullString();
        File.WriteAllText(dbContextFile, dbContextContent);
    }

    /// <summary>
    /// 生成manager
    /// </summary>
    public async Task GenerateMangerAsync(bool force)
    {
        string iManagerDir = Path.Combine(ApplicationPath, "IManager");
        string managerDir = Path.Combine(ApplicationPath, "Manager");
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);

        string interfaceContent = CodeGen.GetIManagerContent(ModuleName);
        string managerContent = CodeGen.GetManagerContent(ModuleName);

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
            string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
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
            var newUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();
            if (newUsings.Any())
            {
                newUsings.Insert(0, Environment.NewLine);
                File.AppendAllLines(filePath, newUsings);
            }
        }
        else
        {
            await GenerateFileAsync(ApplicationPath, "GlobalUsings.cs",
                string.Join(Environment.NewLine, globalUsings));
        }

        var entityFrameworkPath = Path.Combine(SolutionPath, Config.EntityFrameworkPath);
        filePath = Path.Combine(entityFrameworkPath, "GlobalUsings.cs");
        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            var newUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();
            if (newUsings.Any())
            {
                newUsings.Insert(0, Environment.NewLine);
                File.AppendAllLines(filePath, newUsings);
            }
        }
        else
        {
            await GenerateFileAsync(entityFrameworkPath, "GlobalUsings.cs",
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
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        string queryStoreContent = CodeGen.GetStoreContent("Query");
        string commandStoreContent = CodeGen.GetStoreContent("Command");

        await GenerateFileAsync(queryStoreDir, $"{entityName}QueryStore.cs", queryStoreContent);
        await GenerateFileAsync(commandStoreDir, $"{entityName}CommandStore.cs", commandStoreContent);
    }

    /// <summary>
    /// 生成DataStore上下文
    /// </summary>
    public async Task GetDataStoreContextAsync()
    {
        string storeContext = ManagerGenerate.GetDataStoreContext(StorePath, "EntityFramework");
        // 生成仓储上下文
        await GenerateFileAsync(StorePath, "DataStoreContext.cs", storeContext, true);
    }

    /// <summary>
    /// 生成注入服务
    /// </summary>
    /// <returns></returns>
    public async Task GenerateDIExtensionsAsync()
    {
        var content = ManagerGenerate.GetManagerDIExtensions(SolutionPath, "Application");
        await GenerateFileAsync(ApplicationPath, "ManagerServiceCollectionExtensions.cs", content, true);
    }
}
