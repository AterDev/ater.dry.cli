using System.Text.Json;
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
    public string AfterVersion { get; set; }
    public string CurrentVersion { get; set; }

    public UpdateManager(string solutionFilPath, string currentVersion)
    {
        SolutionFilePath = solutionFilPath;
        CurrentVersion = currentVersion;
        AfterVersion = currentVersion;
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
            UpdateExtensionAsync7(solutionPath).Wait();
            UpdateConst7(solutionPath);
            UpdateCustomizeAttribution7(solutionPath);
            var configFilePath = Path.Combine(solutionPath, Config.ConfigFileName);
            if (File.Exists(configFilePath))
            {
                var config = JsonSerializer.Deserialize<ConfigOptions>(File.ReadAllText(configFilePath));
                if (config != null)
                {
                    config.Version = "7.1.0";
                    File.WriteAllText(configFilePath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
                }
                AfterVersion = "7.1.0";
                return true;
            }
        }

        if (version == NuGetVersion.Parse("7.1.0"))
        {
            // 备份
            Console.WriteLine($"🗃️ backup solution to {solutionPath}.origin.bak");
            IOHelper.CopyDirectory(solutionPath, $"{solutionPath}.origin.bak");
            Console.WriteLine($"🚀 Start to update to 8.0.0");
            var res = await UpdateTo8Async(solutionPath);
            if (res)
            {
                AfterVersion = "8.0.0";
                // 重新生成相关代码
                var appDir = Path.Combine(solutionPath, Config.ApplicationPath);
                var applicationName = AssemblyHelper.GetAssemblyName(new DirectoryInfo(appDir));
                ManagerGenerate.GetDataStoreContext(appDir, applicationName!);

                Console.WriteLine("🙌 Updated successed!");
                return true;
            }
            else
            {
                // 恢复
                Console.WriteLine($"🥹 Update version failed, try to recover files!");
                try
                {
                    Directory.Delete(solutionPath, true);
                    Directory.Move($"{solutionPath}.origin.bak", solutionPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Recover failed:{0}, try to close the solution and try again!", ex.Message);
                }
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
        var aterAbstracture = "Ater.Web.Abstracture";

        if (solutionFilePath == null)
        {
            Console.WriteLine("⚠️ can't find sln file");
            return false;
        }

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
                solution.AddExistProject(Path.Combine(destDir, aterAbstracture, $"{aterAbstracture}{Const.CSharpProjectExtention}"));
            }
            else
            {
                Console.WriteLine($"⚠️ can't find {fromDir}");
            }

            // 迁移原Core到新Entity
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
            var dtoAssemblyName = Config.DtoPath.Split(Path.DirectorySeparatorChar).Last();
            var deleteFiles = new string[] {
                 Path.Combine(solutionPath, Config.DtoPath,"Models","PageList.cs"),
                 Path.Combine(solutionPath, Config.DtoPath,"Models","FilterBase.cs"),
            };
            await solution.RemoveFileAsync(dtoAssemblyName, deleteFiles);
            var globalFilePath = Path.Combine(solutionPath, Config.DtoPath, "GlobalUsings.cs");
            File.AppendAllLines(globalFilePath, new List<string>() {
                "global using Ater.Web.Core.Models;"
            });

            // Application修改
            // 结构调整
            var applicationDir = Path.Combine(solutionPath, Config.ApplicationPath);
            var appAssemblyName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Interface", "IDomainManager.cs"),
                Path.Combine(applicationDir, "IManager", "IDomainManager.cs"),
                $"{appAssemblyName}.IManager");

            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Implement", "DataStoreContext.cs"),
                Path.Combine(applicationDir, "DataStoreContext.cs"),
                $"{appAssemblyName}");

            await solution.MoveDocumentAsync(
                appAssemblyName,
                Path.Combine(applicationDir, "Interface", "IUserContext.cs"),
                Path.Combine(applicationDir, "IUserContext.cs"),
                $"{appAssemblyName}");

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

            // 重构项目依赖关系
            var entityProject = solution.GetProject("Entity");
            var coreProject = solution.GetProject(coreName);
            var aterCoreProject = solution.GetProject(aterCoreName);
            var applicationProject = solution.GetProject(appAssemblyName);
            var aterAbstractureProject = solution.GetProject(aterAbstracture);
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
                "global using Ater.Web.Abstracture.Interface;"
            });

            // 业务代码关联修改
            await UpdateUserCodes8Async(solution);

            // 配置文件等
            var configFile = Path.Combine(solutionPath, Config.ConfigFileName);
            var config = JsonSerializer.Deserialize<ConfigOptions>(File.ReadAllText(configFile));
            if (config != null)
            {
                config.EntityPath = $"src{Path.DirectorySeparatorChar}Entity";
                config.Version = "8.0.0";
                config.SolutionType = Core.Entities.SolutionType.DotNet;

                File.WriteAllText(configFile, JsonSerializer.Serialize(config, new JsonSerializerOptions()
                {
                    WriteIndented = true
                }));

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
        }

    }

    /// <summary>
    /// 更新用户业务代码逻辑
    /// </summary>
    private static async Task UpdateUserCodes8Async(SolutionHelper helper)
    {
        var solution = helper.Solution;
        var appName = Config.ApplicationPath.Split(Path.DirectorySeparatorChar).Last();
        var appPath = Path.Combine(solution.FilePath!, "../", Config.ApplicationPath);

        // IDomainManager相关内容
        var content = GenerateBase.GetTplContent("Interface.IDomainManager.tpl");
        content = content.Replace("${Namespace}", appName);
        await IOHelper.WriteToFileAsync(Path.Combine(appPath, "IManager", "IDomainManager.cs"), content);

        content = GenerateBase.GetTplContent("Implement.ManagerBase.tpl");
        await IOHelper.WriteToFileAsync(Path.Combine(appPath, "Implement", "ManagerBase.cs"), content);

        var appProject = helper.GetProject(appName);

        // ManagerBase调整
        var document = appProject?.Documents
            .Where(d => d.Name.Equals("DomainManagerBase"))
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
            .Where(d => d.Folders[0].Equals("IManager"))
            .ToList();
        var compilation = CSharpCompilation.Create("tmp");
        documents?.ForEach(async document =>
        {
            var tree = await document.GetSyntaxTreeAsync();
            var root = await document.GetSyntaxRootAsync();
            if (tree != null && root != null)
            {
                compilation.AddSyntaxTrees(tree);
                var baseInterface = await CSharpAnalysisHelper.GetBaseInterfaceInfoAsync(compilation, tree);
                if (baseInterface != null && baseInterface.Name == "IDomainManager")
                {
                    // 旧接口，需要继承的接口
                    if (baseInterface.TypeArguments.Length > 1)
                    {
                        var editor = await DocumentEditor.CreateAsync(document);
                        var interfaceDeclaration = root.DescendantNodes()
                            .OfType<InterfaceDeclarationSyntax>()
                            .FirstOrDefault();
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

                        editor.ReplaceNode(oldBaseList, newBaseList);

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

                            if (SyntaxFactory.ParseMemberDeclaration(method) is MethodDeclarationSyntax methodNode)
                            {
                                interfaceDeclaration = interfaceDeclaration.AddMembers(methodNode);
                            }
                        }
                        editor.ReplaceNode(root, interfaceDeclaration);
                        var newRoot = editor.GetChangedRoot();
                        await IOHelper.WriteToFileAsync(document.FilePath!, newRoot.ToFullString());
                    }
                }
            }
        });
    }
    #endregion
}
