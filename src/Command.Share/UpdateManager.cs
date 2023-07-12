using System.Text.Json;
using NuGet.Versioning;

namespace Command.Share;
/// <summary>
/// 更新管理
/// </summary>
public class UpdateManager
{
    /// <summary>
    /// 版本更新
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <param name="currentVersion"></param>
    public static async Task<string> UpdateAsync(string solutionPath, string currentVersion)
    {
        var resVersion = currentVersion;
        var version = NuGetVersion.Parse(currentVersion);
        // 7.0->7.1
        if (version == NuGetVersion.Parse("7.0.0"))
        {
            await UpdateExtensionAsync7(solutionPath);
            UpdateConst7(solutionPath);
            UpdateCustomizeAttributionAsync7(solutionPath);
            resVersion = "7.1.0";
        }

        if (version == NuGetVersion.Parse("7.1.0"))
        {
            resVersion = "8.0.0";
        }

        // 更新配置文件
        if (resVersion != currentVersion)
        {
            var configFilePath = Path.Combine(solutionPath, Config.ConfigFileName);
            if (File.Exists(configFilePath))
            {
                var config = JsonSerializer.Deserialize<ConfigOptions>(File.ReadAllText(configFilePath));
                if (config != null)
                {
                    config.Version = resVersion;
                    File.WriteAllText(configFilePath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
                }
            }
        }

        return resVersion;
    }

    #region 7.0->7.1更新

    /// <summary>
    /// 更新扩展方法
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <returns></returns>
    public static async Task UpdateExtensionAsync7(string solutionPath)
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
    public static void UpdateConst7(string solutionPath)
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
                    """, Encoding.UTF8);
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
                    """, Encoding.UTF8);
            }

            Console.WriteLine("🔔 App Const move to Application, please remove it from Core!");
        }
    }

    /// <summary>
    /// 自定义特性文件
    /// </summary>
    /// <param name="solutionPath"></param>
    public static void UpdateCustomizeAttributionAsync7(string solutionPath)
    {
        Console.WriteLine("⬆️ Update customize attributes.");
        var path = Path.Combine(solutionPath, Config.EntityPath, "CustomizeAttribute.cs");
        var oldFile = Path.Combine(solutionPath, Config.EntityPath, "NgPageAttribute.cs");

        if (!File.Exists(path))
        {
            File.Delete(oldFile);
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
            """, Encoding.UTF8);
        }
    }

    #endregion


    #region 7.1更新到8.0
    /// <summary>
    /// 升级到8.0
    /// </summary>
    /// <param name="solutionPath"></param>
    public static void UpdateTo8(string solutionPath)
    {
        // 添加Infrastructure
        // 迁移原Core到新Entity
        // Application修改
        // Share修改
        // 其他
    }
    #endregion
}
