using Microsoft.Extensions.Hosting;

namespace Application;

/// <summary>
/// 应用配置常量
/// </summary>
public static class AppSetting
{
    public const string Components = "Components";
    public const string None = "None";
    public const string Redis = "Redis";
    public const string Memory = "Memory";
    public const string Otlp = "otlp";

    public const string CommandDB = "CommandDb";
    public const string QueryDB = "QueryDb";
    public const string Cache = "Cache";
    public const string CacheInstanceName = "CacheInstanceName";
    public const string Logging = "Logging";
}

/// <summary>
/// 服务注册扩展
/// </summary>
public static partial class AppServiceCollectionExtensions
{
    /// <summary>
    /// 添加默认应用组件
    /// pgsql/redis/otlp
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDefaultComponents(this IHostApplicationBuilder builder)
    {
        builder.AddDbContext();
        builder.Services.AddMemoryCache();
        return builder;
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbContext(this IHostApplicationBuilder builder)
    {
        var path = Path.Combine(AssemblyHelper.GetStudioPath(), ContextBase.DbName);
        builder.Services.AddDbContext<CommandDbContext>(options =>
        {
            options.UseSqlite($"DataSource={path}", _ =>
            {
                _.MigrationsAssembly("AterStudio");
            });
        });

        builder.Services.AddDbContext<QueryDbContext>(options =>
        {
            options.UseSqlite($"DataSource={path}", _ =>
            {
                _.MigrationsAssembly("AterStudio");
            });
        });
        return builder;
    }
}
