using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using Core.Infrastructure;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using NuGet.Versioning;

namespace Command.Share;
/// <summary>
/// 更新管理
/// </summary>
public class UpdateManager
{
    public static string? ErrorMsg { get; private set; }
    public string SolutionFilePath { get; set; }
    public bool Success { get; set; } = false;
    public string TargetVersion { get; set; }
    public string CurrentVersion { get; set; }

    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        TypeInfoResolver = null
    };

    public UpdateManager(string solutionFilPath, string currentVersion)
    {
        SolutionFilePath = solutionFilPath;
        CurrentVersion = currentVersion;
        TargetVersion = currentVersion;
    }

    /// <summary>
    /// 版本更新
    /// </summary>
    /// <param name="solutionFilePath"></param>
    /// <param name="currentVersion"></param>11
    public async Task<bool> UpdateInfrastructureAsync()
    {
        var solutionPath = Path.GetDirectoryName(SolutionFilePath)!;
        var version = NuGetVersion.Parse(CurrentVersion);
        // 7.0->7.1
        if (version == NuGetVersion.Parse("7.0.0"))
        {
            TargetVersion = "7.1.0";
            // 临时修正路径
            Config.EntityPath = "src" + Path.DirectorySeparatorChar + Config.EntityPath;
            Config.SharePath = "src" + Path.DirectorySeparatorChar + Config.SharePath;
            Config.EntityFrameworkPath = "src" + Path.DirectorySeparatorChar + Config.EntityFrameworkPath;
            Config.ApplicationPath = "src" + Path.DirectorySeparatorChar + Config.ApplicationPath;
            Config.ApiPath = "src" + Path.DirectorySeparatorChar + Config.ApiPath;

            UpdateExtensionAsync7(solutionPath).Wait();
            UpdateConst7(solutionPath);
            UpdateCustomizeAttribution7(solutionPath);
            var configFilePath = Path.Combine(solutionPath, Config.ConfigFileName);
            if (File.Exists(configFilePath))
            {
                var config = ConfigOptions.ParseJson(File.ReadAllText(configFilePath));
                if (config != null)
                {
                    config.Version = "7.1.0";
                    // update path 
                    config.EntityPath = Config.EntityPath;
                    config.DtoPath = Config.SharePath;
                    config.DbContextPath = Config.EntityFrameworkPath;
                    config.ApplicationPath = Config.ApplicationPath;
                    config.ApiPath = Config.ApiPath;

                    File.WriteAllText(configFilePath, JsonSerializer.Serialize(config, JsonSerializerOptions));
                }
                Console.WriteLine("🙌 Updated successed!");
                return true;
            }
        }

        if (version == NuGetVersion.Parse("7.1.0"))
        {
            TargetVersion = "8.0.0";
            Console.WriteLine($"🚀 Start to update to 8.0.0");
            var res = await UpdateTo8Async(solutionPath);
            if (res)
            {
                // 重新生成相关代码
                var appDir = Path.Combine(solutionPath, Config.EntityFrameworkPath);
                var applicationName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(appDir));
                ManagerGenerate.GetDataStoreContext(appDir, applicationName!);

                Console.WriteLine("🙌 Updated success!");
                return true;
            }
            else
            {
                // 恢复
                Console.WriteLine($"🥹 Update version failed, try to close the solution and try again!");
            }
        }
        Console.WriteLine("⚠️ Update completed:we encountered some errors!");
        return false;
    }

    #region 7.0->7.1更新

    /// <summary>
    /// 更新扩展方法
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <returns></returns>
    private static async Task UpdateExtensionAsync7(string solutionPath)
    {
        var extensionPath = Path.Combine(solutionPath, Config.EntityPath, "Utils", "Extensions.cs");
        if (File.Exists(extensionPath))
        {
            CompilationHelper compilation = new(Path.Combine(solutionPath, Config.EntityPath));
            compilation.AddSyntaxTree(File.ReadAllText(extensionPath));
            if (!compilation.MethodExist("public static IQueryable<TSource> WhereNotNull<TSource>(this IQueryable<TSource> source, object? field, Expression<Func<TSource, bool>> expression)"))
            {
                string whereNotNullString = """
                    public static IQueryable<TSource> WhereNotNull<TSource>(this IQueryable<TSource> source, object? field, Expression<Func<TSource, bool>> expression)
                    {
                        return field != null ? source.Where(expression) : source;
                    }
                """;
                compilation.InsertClassMethod(whereNotNullString);
                string newClassContent = compilation.SyntaxRoot!.ToString();
                await CommandBase.GenerateFileAsync(
                    Path.Combine(extensionPath, ".."),
                    "Extensions.cs",
                    newClassContent,
                    true);

                await Console.Out.WriteLineAsync("👉 add [WhereNotNull] method to Extension.cs!");
            }
        }
        else
        {
            Console.WriteLine($"⚠️ can't find {extensionPath}");
        }
    }

    /// <summary>
    /// 更新常量文件
    /// </summary>
    private static void UpdateConst7(string solutionPath)
    {
        var applicationPath = Path.Combine(solutionPath, Config.ApplicationPath);
        Console.WriteLine("⬆️ Update app const.");
        string errorMsgPath = Path.Combine(applicationPath, "Const", "ErrorMsg.cs");
        string appConstPath = Path.Combine(applicationPath, "Const", "AppConst.cs");
        if (!File.Exists(errorMsgPath))
        {
            if (!Directory.Exists(Path.Combine(applicationPath, "Const")))
            {
                _ = Directory.CreateDirectory(Path.Combine(applicationPath, "Const"));
            }
            if (!File.Exists(errorMsgPath))
            {
                File.WriteAllText(errorMsgPath, """
                    namespace Application.Const;
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
                    """, new UTF8Encoding(false));
            }
            if (!File.Exists(appConstPath))
            {
                File.WriteAllText(appConstPath, """
                    namespace Application.Const;
                    /// <summary>
                    /// 应用程序常量
                    /// </summary>
                    public static class AppConst
                    {
                        public const string DefaultStateName = "statestore";
                        public const string DefaultPubSubName = "pubsub";

                        /// <summary>
                        /// 管理员policy
                        /// </summary>
                        public const string AdminUser = "AdminUser";
                        /// <summary>
                        /// 普通用户policy
                        /// </summary>
                        public const string User = "User";

                        /// <summary>
                        /// 版本
                        /// </summary>
                        public const string Version = "Version";
                    }
                    """, new UTF8Encoding(false));
            }

            Console.WriteLine("🔔 App Const move to Application, please remove it from Core!");
        }
    }

    /// <summary>
    /// 自定义特性文件
    /// </summary>
    /// <param name="solutionPath"></param>
    private static void UpdateCustomizeAttribution7(string solutionPath)
    {
        Console.WriteLine("⬆️ Update customize attributes.");
        var path = Path.Combine(solutionPath, Config.EntityPath, "CustomizeAttribute.cs");
        var oldFile = Path.Combine(solutionPath, Config.EntityPath, "NgPageAttribute.cs");

        if (!File.Exists(path))
        {
            if (File.Exists(oldFile))
            {
                File.Delete(oldFile);
            }
            File.WriteAllTextAsync(path, """
            namespace Core;

            /// <summary>
            /// 模块标记
            /// </summary>
            [AttributeUsage(AttributeTargets.Class)]
            public class ModuleAttribute : Attribute
            {
                /// <summary>
                /// 模块名称，区分大小写
                /// </summary>
                public string Name { get; init; }

                public ModuleAttribute(string name)
                {
                    Name = name;
                }
            }
            """, new UTF8Encoding(false));
        }
    }

    #endregion

    #region 7.1更新到8.0
    /// <summary>
    /// 升级到8.0
    /// </summary>
    /// <param name="solutionPath"></param>
    public static async Task<bool> UpdateTo8Async(string solutionPath)
    {
        var solutionFilePath = Directory.GetFiles(solutionPath, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();

        var aterCoreName = "Ater.Web.Core";
        var aterAbstraction = "Ater.Web.Abstraction";

        if (solutionFilePath == null)
        {
            Console.WriteLine("⚠️ can't find sln file");
            return false;
        }
        // 先移动文件
        var applicationDir = Path.Combine(solutionPath, Config.ApplicationPath);
        var entityFrameworkDir = Path.Combine(solutionPath, Config.EntityFrameworkPath);
        IOHelper.MoveDirectory(Path.Combine(applicationDir, "QueryStore"), Path.Combine(entityFrameworkDir, "QueryStore"));
        IOHelper.MoveDirectory(Path.Combine(applicationDir, "CommandStore"), Path.Combine(entityFrameworkDir, "CommandStore"));

        var solution = new SolutionHelper(solutionFilePath);
        try
        {
            // 添加Infrastructure
            var studioPath = AssemblyHelper.GetStudioPath();
            var fromDir = Path.Combine(studioPath, "Infrastructure");
            var destDir = Path.Combine(solutionPath, "src", "Infrastructure");
            // copy Infrastructure
            if (Directory.Exists(fromDir))
            {
                if (Directory.Exists(destDir))
                {
                    Directory.Delete(destDir, true);
                }
                Directory.CreateDirectory(destDir);
                IOHelper.CopyDirectory(fromDir, destDir);
                // add to solution
                solution.AddExistProject(Path.Combine(destDir, aterCoreName, $"{aterCoreName}{Const.CSharpProjectExtention}"));
                solution.AddExistProject(Path.Combine(destDir, aterAbstraction, $"{aterAbstraction}{Const.CSharpProjectExtention}"));
            }
            else
            {
                Console.WriteLine($"⚠️ can't find {fromDir}");
            }

            // 迁移原Core到新Entity
            Config.EntityPath = Path.Combine("src", "Core");
            var coreProjectFilePath = Directory.GetFiles(Path.Combine(solutionPath, Config.EntityPath), $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
            var coreName = Config.EntityPath.Split(Path.DirectorySeparatorChar).Last();
            if (coreProjectFilePath == null)
            {
                Console.WriteLine($"Orignal Core project not found:{0}", coreProjectFilePath);
                return false;
            }

            var entitiesDir = Path.Combine(solutionPath, Config.EntityPath, "Entities");
            destDir = Path.Combine(solutionPath, "src", "Entity");

            IOHelper.MoveDirectory(entitiesDir, destDir);

            // move .csproj
            var sourceProjectFile = Directory.GetFiles(Path.Combine(solutionPath, Config.EntityPath), $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            if (sourceProjectFile != null)
            {
                var destProjectFile = Path.Combine(destDir, $"Entity{Const.CSharpProjectExtention}");
                File.Move(sourceProjectFile, destProjectFile, true);
                solution.AddExistProject(destProjectFile);
            }

            // create globaUsings
            var globalUsingPath = Path.Combine(destDir, "GlobalUsings.cs");
            File.WriteAllText(globalUsingPath, $"""
                global using System.ComponentModel;
                global using System.ComponentModel.DataAnnotations;
                global using Ater.Web.Core.Attributes;
                global using Ater.Web.Core.Models;
                global using Microsoft.EntityFrameworkCore;
                """, new UTF8Encoding(false));
            // delete old project
            Directory.Delete(Path.Combine(solutionPath, Config.EntityPath), true);
            // remove attributes
            solution.RemoveAttributes("Entity", "NgPage");

            // Share修改
            var dtoAssemblyName = Config.SharePath.Split(Path.DirectorySeparatorChar).Last();
            var deleteFiles = new string[] {
                 Path.Combine(solutionPath, Config.SharePath,"Models","PageList.cs"),
                 Path.Combine(solutionPath, Config.SharePath,"Models","FilterBase.cs"),
            };
            await solution.RemoveFileAsync(dtoAssemblyName, deleteFiles);
            var globalFilePath = Path.Combine(solutionPath, Config.SharePath, "GlobalUsings.cs");
            File.AppendAllLines(globalFilePath, new List<string>() {
                "global using Ater.Web.Core.Models;"
            });

            // Application修改
            // 结构调整
            var appAssemblyName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
            var dbAseemblyName = Config.EntityFrameworkPath.Split(Path.DirectorySeparatorChar).Last();
            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Interface", "IDomainManager.cs"),
                Path.Combine(applicationDir, "IManager", "IDomainManager.cs"),
                $"{appAssemblyName}.IManager");

            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Implement", "DataStoreContext.cs"),
                Path.Combine(entityFrameworkDir, "DataStoreContext.cs"),
                $"{dbAseemblyName}");

            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Implement", "StoreServicesExtensions.cs"),
                Path.Combine(applicationDir, "ManagerServiceCollectionExtensions.cs"),
                $"{appAssemblyName}");

            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Interface", "IUserContext.cs"),
                Path.Combine(applicationDir, "IUserContext.cs"),
                $"{appAssemblyName}");

            // 模板内容更新
            var content = GenerateBase.GetTplContent("Implement.CommandStoreBase.tpl");
            content = content.Replace("${Namespace}", appAssemblyName);
            await IOHelper.WriteToFileAsync(Path.Combine(applicationDir, "Implement", "CommandStoreBase.cs"), content);
            content = GenerateBase.GetTplContent("Implement.QueryStoreBase.tpl");
            content = content.Replace("${Namespace}", appAssemblyName);
            await IOHelper.WriteToFileAsync(Path.Combine(applicationDir, "Implement", "QueryStoreBase.cs"), content);

            content = GenerateBase.GetTplContent("AppServiceCollectionExtensions.tpl");
            content = content.Replace("${Namespace}", appAssemblyName);
            await IOHelper.WriteToFileAsync(Path.Combine(applicationDir, "AppServiceCollectionExtensions.cs"), content);
            var oldFile = Path.Combine(applicationDir, "ServiceExtension.cs");
            if (File.Exists(oldFile))
            {
                File.Delete(oldFile);
            }

            // remove files
            deleteFiles = Directory.GetFiles(
                Path.Combine(applicationDir, "Interface"),
                "*.cs",
                SearchOption.AllDirectories);
            await solution.RemoveFileAsync(appAssemblyName, deleteFiles ?? Array.Empty<string>());

            // remove package reference
            string[] packageNames = new string[] {
                "Microsoft.IdentityModel.Tokens",
                "Microsoft.AspNetCore.Http.Abstractions",
                "System.IdentityModel.Tokens.Jwt",
                "Microsoft.EntityFrameworkCore.SqlServer",
                "Npgsql.EntityFrameworkCore.PostgreSQL",
                "Microsoft.Extensions.Caching.StackExchangeRedis",
                "OpenTelemetry",
                "OpenTelemetry.Exporter.Console",
                "OpenTelemetry.Exporter.OpenTelemetryProtocol",
                "OpenTelemetry.Exporter.OpenTelemetryProtocol.Logs",
                "OpenTelemetry.Extensions.Hosting",
                "OpenTelemetry.Instrumentation.AspNetCore",
                "OpenTelemetry.Instrumentation.Http",
                "OpenTelemetry.Instrumentation.SqlClient"
            };
            var appProjectFile = Directory.GetFiles(applicationDir, $"*{Const.CSharpProjectExtention}", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (appProjectFile != null)
            {
                var commands = packageNames.Select(p => $"dotnet remove {appProjectFile} package " + p).ToArray();
                ProcessHelper.ExecuteCommands(commands);
            }

            // rename namespace
            solution.RenameNamespace("Core.Entities", "Entity");
            solution.RenameNamespace("Core.Models", "Ater.Web.Core.Models");
            solution.RenameNamespace("Core.Utils", "Ater.Web.Core.Utils");
            solution.RenameNamespace("Application.Interface", "Application");
            solution.RenameNamespace("Application.QueryStore", "EntityFramework.QueryStore");
            solution.RenameNamespace("Application.CommandStore", "EntityFramework.CommandStore");

            // 重构项目依赖关系
            var entityProject = solution.GetProject("Entity");
            var coreProject = solution.GetProject(coreName);
            var aterCoreProject = solution.GetProject(aterCoreName);
            var applicationProject = solution.GetProject(appAssemblyName);
            var aterAbstractureProject = solution.GetProject(aterAbstraction);
            var dtoProject = solution.GetProject(dtoAssemblyName);
            var entityFrameworkProject = solution.GetProject("EntityFramework");

            if (entityProject == null || aterCoreProject == null || applicationProject == null || aterAbstractureProject == null || dtoProject == null || entityFrameworkProject == null
                || coreProject == null)
            {
                Console.WriteLine("⚠️ 项目依赖关系重构失败，缺失的项目关系:" +
                    "\nentityProject:" + entityProject?.Name +
                    "\ncoreProject:" + coreProject?.Name +
                    "\naterCoreProject:" + aterCoreProject?.Name +
                    "\napplicationProject:" + applicationProject?.Name +
                    "\naterAbstractureProject:" + aterAbstractureProject?.Name +
                    "\ndtoProject:" + dtoProject?.Name +
                    "\nentityFrameworkProject:" + entityFrameworkProject?.Name
                    );
                return false;
            }

            // 原依赖Core的改成Entity
            var originProjects = solution.GetReferenceProject(coreName);
            originProjects?.ForEach(p =>
            {
                solution.RemoveProjectReference(p, coreProject);
                solution.AddProjectReference(p, entityProject);
            });
            // 其他依赖
            solution.RemoveProject(Path.GetFileNameWithoutExtension(coreProjectFilePath));
            solution.AddProjectReference(entityProject, aterCoreProject);
            solution.AddProjectReference(applicationProject, aterAbstractureProject);

            globalFilePath = Path.Combine(solutionPath, Config.ApplicationPath, "GlobalUsings.cs");
            File.AppendAllLines(globalFilePath, new List<string>() {
                "global using Ater.Web.Abstraction.Interface;"
            });

            // 业务代码关联修改
            await UpdateUserCodes8Async(solution);
            UpdateAppsettings(solutionPath);


            // 配置文件等
            var configFile = Path.Combine(solutionPath, Config.ConfigFileName);
            var config = ConfigOptions.ParseJson(File.ReadAllText(configFile));
            if (config != null)
            {
                config.EntityPath = $"src{Path.DirectorySeparatorChar}Entity";
                config.Version = "8.0.0";
                config.SolutionType = Core.Entities.SolutionType.DotNet;

                File.WriteAllText(configFile, JsonSerializer.Serialize(config, JsonSerializerOptions));

                Config.SetConfig(config);
                solution.Dispose();
                return true;
            }
            solution.Dispose();
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
            return false;
        }
        finally
        {
            solution.Dispose();
            GC.Collect();
        }
    }

    /// <summary>
    /// 更新用户业务代码逻辑
    /// </summary>
    private static async Task UpdateUserCodes8Async(SolutionHelper helper)
    {
        var solution = helper.Solution;
        var appName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
        var appPath = Path.Combine(solution.FilePath!, "..", Config.ApplicationPath);

        // IDomainManager相关内容
        var content = GenerateBase.GetTplContent("Interface.IDomainManager.tpl");
        content = content.Replace("${Namespace}", appName);
        await IOHelper.WriteToFileAsync(Path.Combine(appPath, "IManager", "IDomainManager.cs"), content);

        content = GenerateBase.GetTplContent("Implement.ManagerBase.tpl");
        await IOHelper.WriteToFileAsync(Path.Combine(appPath, "Implement", "ManagerBase.cs"), content);

        var appProject = helper.GetProject(appName);
        // ManagerBase调整
        var document = appProject?.Documents
            .Where(d => d.Name.Equals("DomainManagerBase.cs"))
            .FirstOrDefault();
        if (document != null && document.FilePath != null)
        {
            var root = await document.GetSyntaxRootAsync();
            var classNode = root?.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classNode != null)
            {
                var rightBase = classNode.BaseList?.Types
                    .Any(t => t.Type.ToString().Equals("ManagerBase")) ?? false;
                if (!rightBase)
                {
                    content = GenerateBase.GetTplContent("Implement.DomainManagerBase.tpl");
                    await IOHelper.WriteToFileAsync(document.FilePath!, content);
                }
            }
        }
        // IManager调整
        var documents = appProject?.Documents
            .Where(d => d.Folders.Any() && d.Folders[0].Equals("IManager"))
            .ToList();
        if (documents != null)
        {
            await RefactorIManagers(appPath, documents);
        }

        // Controllers调整
        var apiName = Config.ApiPath.Split(Path.DirectorySeparatorChar).Last();
        var apiPath = Path.Combine(solution.FilePath!, "..", Config.ApiPath);
        var apiProject = helper.GetProject(apiName);
        var controllers = apiProject?.Documents
            .Where(d => d.Folders.Any() && d.Folders[0].Equals("Controllers"))
            .Where(d => d.FilePath != null && d.FilePath.EndsWith("Controller.cs"))
            .ToList();
        if (controllers != null)
        {
            await RefactorControllersAsync(apiPath, controllers);
        }
        // RestControllerBase更新
        var baseFile = Path.Combine(apiPath, "Infrastructure", "RestControllerBase.cs");
        content = await File.ReadAllTextAsync(baseFile);
        if (!content.Contains("public class ClientControllerBase<TManager>"))
        {
            content = content + Environment.NewLine +
                """
                /// <summary>
                /// 用户端权限控制器
                /// </summary>
                /// <typeparam name="TManager"></typeparam>
                [Authorize(AppConst.User)]
                [ApiExplorerSettings(GroupName = "client")]
                public class ClientControllerBase<TManager> : RestControllerBase
                     where TManager : class
                {
                    protected readonly TManager manager;
                    protected readonly ILogger _logger;
                    protected readonly IUserContext _user;

                    public ClientControllerBase(
                        TManager manager,
                        IUserContext user,
                        ILogger logger
                        )
                    {
                        this.manager = manager;
                        _user = user;
                        _logger = logger;
                    }
                }
                """;
        }
        await IOHelper.WriteToFileAsync(baseFile, content);
    }

    /// <summary>
    /// 重构Controller内容
    /// </summary>
    /// <param name="apiPath"></param>
    /// <param name="documents"></param>
    private static async Task RefactorControllersAsync(string apiPath, List<Document> documents)
    {
        var dlls = Directory.EnumerateFiles(apiPath, "*.dll", SearchOption.AllDirectories);
        var compilation = CSharpCompilation.Create("tmp")
            .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var item in documents)
        {
            var tree = await item.GetSyntaxTreeAsync();
            var root = await item.GetSyntaxRootAsync();
            if (tree != null && root != null)
            {
                var content = root.ToFullString();
                content = content.Replace("await manager.GetCurrent(", "await manager.GetCurrentAsync(");
                await IOHelper.WriteToFileAsync(item.FilePath!, content);
            }
        }
    }

    /// <summary>
    /// 重构Manager接口
    /// </summary>
    /// <param name="appPath"></param>
    /// <param name="documents"></param>
    /// <returns></returns>
    private static async Task RefactorIManagers(string appPath, List<Document> documents)
    {
        var dlls = Directory.EnumerateFiles(appPath, "*.dll", SearchOption.AllDirectories);
        var compilation = CSharpCompilation.Create("tmp")
            .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        foreach (var item in documents)
        {
            var tree = await item.GetSyntaxTreeAsync();
            var root = await item.GetSyntaxRootAsync();
            if (tree != null && root != null)
            {

                var baseInterface = await CSharpAnalysisHelper.GetBaseInterfaceInfoAsync(compilation, tree);
                if (baseInterface != null && baseInterface.Name == "IDomainManager")
                {
                    // 旧接口，需要继承的接口
                    if (baseInterface.TypeArguments.Length > 1)
                    {
                        var editor = await DocumentEditor.CreateAsync(item);
                        var originInterfaceDeclaration = root.DescendantNodes()
                            .OfType<InterfaceDeclarationSyntax>()
                            .FirstOrDefault();

                        var interfaceDeclaration = originInterfaceDeclaration;
                        var oldBaseList = interfaceDeclaration!.DescendantNodes()
                            .OfType<BaseListSyntax>()
                            .Single();

                        var firstTypeName = baseInterface.TypeArguments[0].Name;
                        var typeName = SyntaxFactory.ParseTypeName($"IDomainManager<{firstTypeName}>");
                        var baseType = SyntaxFactory.SimpleBaseType(typeName);
                        var newColonToken = SyntaxFactory.Token(SyntaxKind.ColonToken)
                          .WithTrailingTrivia(SyntaxFactory.Space);
                        var newBaseList = SyntaxFactory.BaseList(
                          SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(baseType))
                              .WithTrailingTrivia(SyntaxFactory.LineFeed)
                              .WithColonToken(newColonToken);

                        interfaceDeclaration = interfaceDeclaration.ReplaceNode(oldBaseList, newBaseList);

                        // 插入接口方法
                        var entityName = firstTypeName;
                        string[] methods = new string[]{
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

                        foreach (string method in methods)
                        {
                            // 如果已有接口中的方法，则不再添加
                            if (root.DescendantNodes().Where(r => r is MethodDeclarationSyntax)
                                .Any(m => m.ToString().Contains(method)))
                            {
                                continue;
                            }
                            var methodContent = method + Environment.NewLine;
                            if (SyntaxFactory.ParseMemberDeclaration(methodContent) is MethodDeclarationSyntax methodNode)
                            {
                                interfaceDeclaration = interfaceDeclaration.AddMembers(methodNode);
                            }
                        }
                        editor.ReplaceNode(originInterfaceDeclaration!, interfaceDeclaration);
                        var newRoot = editor.GetChangedRoot();
                        await IOHelper.WriteToFileAsync(item.FilePath!, CSharpAnalysisHelper.FormatChanges(newRoot));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 配置文件更新
    /// </summary>
    /// <param name="solutionPath"></param>
    private static void UpdateAppsettings(string solutionPath)
    {
        var apiPath = Path.Combine(solutionPath, Config.ApiPath);
        var appSettingPath = Path.Combine(apiPath, "appsettings.json");
        var content = File.ReadAllText(appSettingPath);
        var root = JsonNode.Parse(content, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip
        });

        if (root != null)
        {
            // 原内容
            var connectionNode = JsonHelper.GetSectionNode(root, "ConnectionStrings");
            if (connectionNode != null)
            {
                var dbConnectionString = JsonHelper.GetValue<string>(connectionNode, "Default");
                var redisConnectionString = JsonHelper.GetValue<string>(connectionNode, "Redis");
                var redisInstanceName = JsonHelper.GetValue<string>(connectionNode, "RedisInstanceName");

                var defaultDbConnectionString = "Server=localhost;Port=5432;Database=MyProjectName;User Id=postgres;Password=root;";
                JsonHelper.AddOrUpdateJsonNode(root, "ConnectionStrings:CommandDb", dbConnectionString ?? defaultDbConnectionString);
                JsonHelper.AddOrUpdateJsonNode(root, "ConnectionStrings:QueryDb", dbConnectionString ?? defaultDbConnectionString);

                JsonHelper.AddOrUpdateJsonNode(root, "ConnectionStrings:Cache", redisConnectionString ?? "localhost:6379");
                JsonHelper.AddOrUpdateJsonNode(root, "ConnectionStrings:CacheInstanceName", redisInstanceName ?? "Dev");
                JsonHelper.AddOrUpdateJsonNode(root, "ConnectionStrings:Logging", "http://localhost:4317");
            }
            JsonHelper.AddOrUpdateJsonNode(root, "Components:Database", "postgresql");
            JsonHelper.AddOrUpdateJsonNode(root, "Components:Cache", "redis");
            JsonHelper.AddOrUpdateJsonNode(root, "Components:Logging", "none");
            JsonHelper.AddOrUpdateJsonNode(root, "Components:Swagger", true);
            JsonHelper.AddOrUpdateJsonNode(root, "Components:Jwt", true);

            content = root.ToJsonString(JsonSerializerOptions);
            File.WriteAllText(appSettingPath, content);
        }
    }

    /// <summary>
    /// TODO:入口程序更新
    /// </summary>
    public static void UpdateProgram(string apiPath)
    {
        var programPath = Path.Combine(apiPath, "Program.cs");
        var compilation = CSharpCompilation.Create(
            "tmp", syntaxTrees: new[] { CSharpSyntaxTree.ParseText(File.ReadAllText(programPath)) },
            references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var tree = compilation.SyntaxTrees[0];
        var root = tree.GetCompilationUnitRoot();
        var model = compilation.GetSemanticModel(tree);

        // 添加或删除的方法  services.AddTransient<IUserContext, UserContext>();
        var addServices = new List<string>() { "AddAppComponents", "AddWebComponent", "AddHttpContextAccessor" };
        var removeServices = new string[]
        {
            "AddDbContextPool","AddOpenTelemetry",
            "AddStackExchangeRedisCache","AddAuthentication",
            "AddSwaggerGen","AddEndpointsApiExplorer"
        };
    }

    /// <summary>
    /// 更新结果 
    /// </summary>
    /// <param name="isSuccess">是否成功</param>
    public string GetUpdateNotes(bool isSuccess)
    {
        var content = $"""
            ## 更新结果

            更新前版本:{CurrentVersion}

            更新后版本:{TargetVersion}

            ## 结果

            {(isSuccess ? "👍 更新成功" : "😢 更新失败")}

            ## 后续步骤

            {(!isSuccess ?
            """
            - 清理或回滚更新产生的变更！
            - 查看控制台错误信息，并将更新错误信息反馈到 [Github Issue](https://github.com/AterDev/ater.dry.cli/issues)。
            """
            :
            """
            更新完成后，你可能需要处理以下内容：

            对于Web接口项目

            - 你可能需要在`Program.cs`中添加服务注册

                ```csharp
                services.AddHttpContextAccessor(); 
                services.AddTransient<IUserContext, UserContext>();
                ```

            - 你可以使用新的扩展方法注入服务组件，如

                ```csharp
                services.AddAppComponents(builder.Configuration);
                services.AddWebComponent(builder.Configuration);
                ```

                以替代并简化
                `AddStackExchangeRedisCache`,
                `AddAuthentication`,
                `AddEndpointsApiExplorer`,
                `AddSwaggerGen`
                等方法。

            - 你可以在`Infrastructure/ServiceCollectionExtension.cs`配置相应的服务。
            - 查看`appsettings.json`的变更，更新并配置`Components`节点内容。
            - 依据`appsettings.json`中的内容，更新开发和生产的配置文件。

            所有项目
            - 如果跨主版本更新，如7.x->8.0，需要升级到对应的.NET版本以及依赖包版本。
            - 根据IDE的提示修复其他命名或引用问题。

            更多信息查看官方文档[模板]相关的说明。
            
            """)}
            
            """;
        return content;
    }
    #endregion

    /// <summary>
    /// 移除Manager的接口层
    /// </summary>
    public static async Task<bool> RemoveManagerInterfaceAsync(string solutionPath)
    {
        var solutionFilePath = Directory.GetFiles(solutionPath, "*.sln", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if (solutionFilePath == null)
        {
            Console.WriteLine("⚠️ can't find sln file");
            return false;
        }

        var appName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
        var helper = new SolutionHelper(solutionFilePath);

        var appProject = helper.GetProject(appName);
        var solution = helper.Solution;

        // 更新 controllers
        var controllerFiles = Directory.GetFiles(solutionPath, "*Controller.cs", SearchOption.AllDirectories)
            .ToList();
        controllerFiles.ForEach(file =>
        {
            var content = File.ReadAllText(file);
            var pattern = @"I(\w+)Manager";
            var matches = Regex.Matches(content, pattern);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var oldName = match.Groups[0].Value;
                    var newName = match.Groups[1].Value + "Manager";
                    content = content.Replace(oldName, newName);
                    Console.WriteLine($"⛏️ update [{Path.GetFileName(file)}]:{oldName}=>{newName}");
                }
                File.WriteAllText(file, content);
            }
        });

        // 更新 Manager
        var managers = helper.Solution.Projects.SelectMany(p => p.Documents)
            .Where(d => d.Folders.Any() && d.Folders.Any(f => f.Equals("Manager")))
            .Where(d => d.FilePath != null && d.FilePath.EndsWith("Manager.cs"))
            .ToList();


        var dlls = Directory.EnumerateFiles(solutionPath, "*.dll", SearchOption.AllDirectories);
        var compilation = CSharpCompilation.Create("tmp")
            .AddReferences(dlls.Select(dll => MetadataReference.CreateFromFile(dll)))
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        foreach (var manager in managers)
        {
            var tree = await manager.GetSyntaxTreeAsync();
            var root = await manager.GetSyntaxRootAsync();
            if (tree != null && root != null)
            {
                compilation = compilation.AddSyntaxTrees(tree);
                var classDeclaration = root.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault();

                var entityName = manager.Name.Replace("Manager.cs", "");
                var oldInterfaceName = $"I{entityName}Manager";

                var semanticModel = compilation.GetSemanticModel(tree);
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration!);
                var baseInterfaceType = classSymbol!.Interfaces
                    .Where(i => i.Name.Equals(oldInterfaceName))
                    .FirstOrDefault();

                if (baseInterfaceType == null)
                {
                    Console.WriteLine($"🦘 skip {manager.Name} ");
                    continue;
                }

                var content = root.ToFullString();
                if (baseInterfaceType != null)
                {
                    var editor = await DocumentEditor.CreateAsync(manager);

                    var oldNode = classDeclaration!.DescendantNodes().OfType<SimpleBaseTypeSyntax>()
                        .Where(n => n.Type.ToString().Equals(oldInterfaceName))
                        .Single();


                    var typeName = SyntaxFactory.ParseTypeName($"IDomainManager<{entityName}>");
                    var baseType = SyntaxFactory.SimpleBaseType(typeName);


                    var newNode = baseType.WithTrailingTrivia(SyntaxFactory.LineFeed);

                    var newClassDeclaration = classDeclaration?.ReplaceNode(oldNode, newNode);
                    if (newClassDeclaration != null)
                    {
                        editor.ReplaceNode(classDeclaration!, newClassDeclaration);
                    }
                    var newRoot = editor.GetChangedRoot();

                    content = CSharpAnalysisHelper.FormatChanges(newRoot);
                }

                var pattern = @"I(\w+)Manager";
                var matches = Regex.Matches(content, pattern);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var oldName = match.Groups[0].Value;
                        if (oldName == "IDomainManager")
                        {
                            continue;
                        }
                        var newName = match.Groups[1].Value + "Manager";
                        content = content.Replace(oldName, newName);
                        Console.WriteLine($"⛏️ update [{Path.GetFileName(manager.FilePath)}]:{oldName}=>{newName}");
                    }
                }
                await IOHelper.WriteToFileAsync(manager.FilePath!, content);
            }
        }

        // 删除所有IManager
        var imanagerFiles = Directory.GetFiles(solutionPath, "I*Manager.cs", SearchOption.AllDirectories)
            .ToList();
        imanagerFiles.ForEach(file =>
        {
            if (!file.EndsWith("IDomainManager.cs"))
            {
                File.Delete(file);
            }
        });

        // 更新 global usings
        var globalUsings = helper.Solution.Projects
            .SelectMany(p => p.Documents)
            .Where(d => d.FilePath != null && d.FilePath.EndsWith("GlobalUsings.cs"))
            .ToList();

        foreach (var g in globalUsings)
        {
            var root = await g.GetSyntaxRootAsync();
            var content = root!.ToFullString();

            var projectName = g.Project.Name;
            if (projectName is "Http.API" or "Application")
            {
                if (!content.Contains("global using Application.Manager;"))
                {
                    content += Environment.NewLine + "global using Application.Manager;";
                }
            }
            if (g.Project.FilePath != null && g.Project.FilePath.Contains("Modules"))
            {

                if (!content.Contains("global using Application.Manager;"))
                {
                    content += Environment.NewLine + "global using Application.Manager;";
                }

                var replaceName = projectName + ".IManager;";
                if (content.Contains(replaceName))
                {
                    content = content.Replace(replaceName, projectName + ".Manager;");
                }
            }

            await IOHelper.WriteToFileAsync(g.FilePath!, content);
            Console.WriteLine($"⛏️ update [{Path.GetFileName(g.FilePath)}]");
        }

        // 重新生成注入服务
        var serviceContent = ManagerGenerate.GetManagerDIExtensions(solutionPath, "Application");
        var applicationDir = Path.Combine(solutionPath, Config.ApplicationPath);
        var filePath = Path.Combine(applicationDir, "ManagerServiceCollectionExtensions.cs");
        await IOHelper.WriteToFileAsync(filePath, serviceContent);
        Console.WriteLine($"⛏️ update [{filePath}]");

        if (Directory.Exists(Path.Combine(solutionPath, "src", "Modules")))
        {
            var moduleDirs = Directory.GetDirectories(
            Path.Combine(solutionPath, "src", "Modules"),
            "*",
             SearchOption.TopDirectoryOnly)
            .ToList();

            if (moduleDirs.Count != 0)
            {
                foreach (var module in moduleDirs)
                {
                    var moduleName = Path.GetFileName(module);
                    serviceContent = ManagerGenerate.GetManagerModuleDIExtensions(solutionPath, moduleName!);

                    filePath = Path.Combine(module, "ServiceCollectionExtensions.cs");
                    await IOHelper.WriteToFileAsync(filePath, serviceContent);
                    Console.WriteLine($"⛏️ update [{filePath}]");

                }
            }
        }

        return true;
    }
}
