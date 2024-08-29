using PluralizeService.Core;

namespace Command.Share.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerCommand : CommandBase
{
    public string EntityFilePath { get; }
    public string ApplicationPath { get; private set; }
    public string SharePath { get; private set; }
    public string StorePath { get; }
    public string SolutionPath { get; }
    public ManagerGenerate? CodeGen { get; set; }
    /// <summary>
    /// 对应模块名
    /// </summary>
    public string? ModuleName { get; private set; }

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
            var compilation = new CompilationHelper(ApplicationPath, "Entity");
            var content = File.ReadAllText(EntityFilePath);
            compilation.LoadContent(content);
            var attributes = compilation.GetClassAttribution("Module");
            if (attributes != null && attributes.Count != 0)
            {
                var argument = attributes.First().ArgumentList!.Arguments[0];
                ModuleName = compilation.GetArgumentValue(argument);
            }
            // 生成到模块项目中
            if (!string.IsNullOrWhiteSpace(ModuleName))
            {
                var modulePath = Path.Combine(ApplicationPath, "..", "Modules", ModuleName);
                if (!Directory.Exists(ApplicationPath))
                {
                    Console.WriteLine($"⚠️ module {ModuleName} not exist, please create first!");
                    return;
                }
                SharePath = Path.Combine(SolutionPath, "src", "Modules", ModuleName);

                CodeGen = new ManagerGenerate(EntityFilePath, SharePath, modulePath);
                await Console.Out.WriteLineAsync($"🆕 generate Manager files");
                await GenerateMangerAsync(SharePath, force);

                await Console.Out.WriteLineAsync($"🆕 Update Module DependencyInject files");
                await GenerateModuleDIExtensionsAsync();

                await Console.Out.WriteLineAsync($"🆕 Update GlobalUsing files");
                await GenerateGlobalUsingsFilesAsync();
            }
            else
            {
                CodeGen = new ManagerGenerate(EntityFilePath, SharePath, ApplicationPath);

                await Console.Out.WriteLineAsync($"🆕 generate Manager files");
                await GenerateMangerAsync(force: force);

                await Console.Out.WriteLineAsync($"🆕 Update DependencyInject files");
                await GenerateDIExtensionsAsync();

                await Console.Out.WriteLineAsync($"🆕 Update GlobalUsing files");
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
    /// 生成模块注入服务扩展
    /// </summary>
    /// <returns></returns>
    private async Task GenerateModuleDIExtensionsAsync()
    {
        var modulePath = Path.Combine(ApplicationPath, "..", "Modules", ModuleName!);
        var content = ManagerGenerate.GetManagerModuleDIExtensions(SolutionPath, ModuleName!);
        await GenerateFileAsync(modulePath, "ServiceCollectionExtensions.cs", content, true);
    }

    /// <summary>
    /// 添加实体到数据库上下文
    /// </summary>
    /// <returns></returns>
    public void AddToDbContext(string entityFrameworkPath)
    {
        Console.WriteLine("🚀 update ContextBase DbSet");
        var dbContextFile = Path.Combine(entityFrameworkPath, "DBProvider", "ContextBase.cs");

        if (!File.Exists(dbContextFile))
        {
            Console.WriteLine($"  ⚠️ Not found:{dbContextFile}");
            return;
        }
        var dbContextContent = File.ReadAllText(dbContextFile);

        var compilation = new CompilationHelper(entityFrameworkPath);
        compilation.LoadContent(dbContextContent);

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
    public async Task GenerateMangerAsync(string? appPath = null, bool force = false)
    {
        appPath ??= ApplicationPath;
        string managerDir = Path.Combine(appPath, "Manager");
        string entityName = Path.GetFileNameWithoutExtension(EntityFilePath);
        string managerContent = CodeGen!.GetManagerContent(ModuleName);

        // 生成manger
        await GenerateFileAsync(managerDir, $"{entityName}Manager.cs", managerContent, force);
    }

    /// <summary>
    /// 生成全局依赖文件GlobalUsings.cs
    /// </summary>
    /// <returns></returns>
    public async Task GenerateGlobalUsingsFilesAsync()
    {
        if (Config.IsMicroservice) { return; }

        List<string> globalUsings = CodeGen!.GetGlobalUsings();
        string filePath = Path.Combine(ApplicationPath, "GlobalUsings.cs");
        if (!string.IsNullOrWhiteSpace(ModuleName))
        {
            filePath = Path.Combine(SolutionPath, "src", "Modules", ModuleName, "GlobalUsings.cs");
        }

        // 如果不存在则生成，如果存在，则添加
        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            var newUsings = globalUsings.Where(g => !content.Contains(g))
                .ToList();
            if (newUsings.Count != 0)
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
    }

    /// <summary>
    /// 生成注入服务
    /// </summary>
    /// <returns></returns>
    public async Task GenerateDIExtensionsAsync()
    {
        var applicationNamespace = AssemblyHelper.GetNamespaceByPath(Config.ApplicationPath);
        var nsp = Config.IsMicroservice ? Config.ServiceName + $".{applicationNamespace}" : applicationNamespace;
        var content = ManagerGenerate.GetManagerDIExtensions(ApplicationPath, nsp);
        await GenerateFileAsync(ApplicationPath, "ManagerServiceCollectionExtensions.cs", content, true);
    }
}
