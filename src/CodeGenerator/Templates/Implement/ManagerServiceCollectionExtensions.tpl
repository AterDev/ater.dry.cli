﻿// 本文件由 ater.dry工具自动生成.
namespace #@Namespace#;

public static partial class ManagerServiceCollectionExtensions
{
    public static void AddManager(this IServiceCollection services)
    {
        services.AddScoped(typeof(DataAccessContext<>));
#@ManagerServices#
    }
}
