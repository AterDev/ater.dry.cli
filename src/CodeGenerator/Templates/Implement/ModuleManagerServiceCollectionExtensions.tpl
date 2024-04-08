// 本文件由 ater.dry工具自动生成.
using ${Namespace}.Manager;

namespace ${Namespace};
/// <summary>
/// 服务注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加模块服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection Add${Namespace}Services(this IServiceCollection services)
    {
        services.Add${Namespace}Managers();
        // add other services
        return services;
    }


    /// <summary>
    /// 添加${Namespace} 注入服务
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection Add${Namespace}Managers(this IServiceCollection services)
    {
${ManagerServices}
        return services;
    }
}

