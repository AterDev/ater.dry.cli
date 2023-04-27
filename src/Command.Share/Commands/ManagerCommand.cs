using System.Diagnostics.CodeAnalysis;
using Core.Infrastructure;
using Core.Models;
using NuGet.Versioning;

namespace Command.Share.Commands;

/// <summary>
/// 数据仓储生成
/// </summary>
public class ManagerCommand : CommandBase
{
    public string EntityPath { get; set; }
    public string StorePath { get; set; }
    public string DtoPath { get; set; }
    public ManagerGenerate CodeGen { get; set; }

    public ManagerCommand(string entityPath, string dtoPath, string servicePath, string? contextName = null)
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
    public async Task RunAsync(bool force)
    {
        if (!File.Exists(EntityPath))
        {
            Console.WriteLine($"the {EntityPath} not exist");
            return;
        }
        await UpdateFilesAsync();
        Console.WriteLine(Instructions[0]);
        await GenerateCommonFilesAsync();
        Console.WriteLine(Instructions[1]);
        await GenerateStoreFilesAsync();

        Console.WriteLine(Instructions[2]);
        await GenerateMangerAsync(force);
        Console.WriteLine(Instructions[3]);
        await GenerateMangerTestAsync(force);

        Console.WriteLine(Instructions[4]);
        await GenerateServicesAsync();

        Console.WriteLine(Instructions[5]);
        await GenerateGlobalUsingsFilesAsync();

        Console.WriteLine("😀 Manager generate completed!" + Environment.NewLine);
    }

    /// <summary>
    /// 待更新的内容
    /// </summary>
    private async Task UpdateFilesAsync()
    {
        if (AssemblyHelper.NeedUpdate(Const.Version))
        {
            // 更新扩展方法
            string updateContent = "";
            Console.WriteLine("🆕 need update base infrastructure.");
            var whereNotNullString = """
                    public static IQueryable<TSource> WhereNotNull<TSource>(this IQueryable<TSource> source, object? field, Expression<Func<TSource, bool>> expression)
                    {
                        return field != null ? source.Where(expression) : source;
                    }
                """;
            // update extension class
            var extensionPath = Path.Combine(StorePath, "..", Config.EntityPath, "Utils", "Extensions.cs");

            if (File.Exists(extensionPath))
            {
                var compilation = new CompilationHelper(Path.Combine(EntityPath, ".."));
                compilation.AddSyntaxTree(File.ReadAllText(extensionPath));
                if (!compilation.MehtodExist("public static IQueryable<TSource> WhereNotNull<TSource>(this IQueryable<TSource> source, object? field, Expression<Func<TSource, bool>> expression)"))
                {
                    compilation.InsertClassMethod(whereNotNullString);

                    var newClassContent = compilation.SyntaxRoot!.ToString();
                    await GenerateFileAsync(Path.Combine(extensionPath, ".."), $"Extensions.cs", newClassContent, true);

                    updateContent += "👉 add [WhereNotNull] method to Extension.cs!" + Environment.NewLine;
                }
            }
            else
            {
                Console.WriteLine($"⚠️ can't find {extensionPath}");
            }
            // 更新Error Const 常量
            var errorMsgPath = Path.Combine(StorePath, "..", Config.EntityPath, "Const", "ErrorMsg.cs");
            if (!File.Exists(errorMsgPath))
            {
                File.WriteAllText(errorMsgPath, """
                    namespace Core.Const;
                    /// <summary>
                    /// 错误信息
                    /// </summary>
                    public static class ErrorMsg
                    {
                        /// <summary>
                        /// 未找到该用户
                        /// </summary>
                        public const string NotFoundUser = "未找到该用户!";
                        /// <summary>
                        /// 未找到的资源
                        /// </summary>
                        public const string NotFoundResource = "未找到的资源!";
                    }

                    """);

                updateContent += "👉 add ErrorMsg.cs!" + Environment.NewLine;
            }

            updateContent += "update finish!";
            Console.WriteLine(updateContent);
        }
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

        var cover = false;
        // 生成接口文件
        foreach (string name in interfaceFiles)
        {
            string content = CodeGen.GetInterfaceFile(name);
            // 更新需要覆盖的文件
            if (AssemblyHelper.NeedUpdate("7.0.0")
                && name == "IDomainManager")
            {
                cover = true;
            }
            // 不可覆盖的文件
            cover = name == "IUserContext" ? false : true;
            await GenerateFileAsync(interfaceDir, $"{name}.cs", content, cover);
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
    public async Task GenerateMangerAsync(bool force)
    {
        string iManagerDir = Path.Combine(StorePath, "IManager");
        string managerDir = Path.Combine(StorePath, "Manager");
        string entityName = Path.GetFileNameWithoutExtension(EntityPath);

        string interfaceContent = CodeGen.GetIManagerContent();
        string managerContent = CodeGen.GetManagerContent();

        // 如果文件已经存在，并且没有选择覆盖，并且符合更新要求，则进行更新
        var iManagerPath = Path.Combine(iManagerDir, $"I{entityName}Manager.cs");
        if (!force
            && File.Exists(iManagerPath)
            && AssemblyHelper.NeedUpdate(Const.Version))
        {
            // update files
            var compilation = new CompilationHelper(StorePath);
            var content = await File.ReadAllTextAsync(iManagerPath);
            compilation.AddSyntaxTree(content);
            // 构造更新的内容
            var methods = new string[]{
                $"Task<{entityName}?> GetCurrentAsync(Guid id, params string[] navigations);",
                $"Task<{entityName}> AddAsync({entityName} entity);",
                $"Task<{entityName}> UpdateAsync({entityName} entity, {entityName}UpdateDto dto);",
                $"Task<{entityName}?> FindAsync(Guid id);",
                $"Task<TDto?> FindAsync<TDto>(Expression<Func<{entityName}, bool>>? whereExp) where TDto : class;",
                $"Task<List<TDto>> ListAsync<TDto>(Expression<Func<{entityName}, bool>>? whereExp) where TDto : class;",
                $"Task<PageList<{entityName}ItemDto>> FilterAsync({entityName}FilterDto filter);",
                $"Task<{entityName}?> DeleteAsync({entityName} entity, bool softDelete = true);",
                $"Task<bool> ExistAsync(Guid id);",
            };

            foreach (var method in methods)
            {
                if (!compilation.MehtodExist(method))
                {
                    compilation.InsertInteraceMethod(method);
                }
            }
            compilation.ReplaceImplement($"IDomainManager<{entityName}>");
            interfaceContent = compilation.SyntaxRoot!.ToString();
            await GenerateFileAsync(iManagerDir, $"I{entityName}Manager.cs", interfaceContent, true);
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
