// 本文件由 ater.dry工具自动生成.
namespace Application;

public static partial class ManagerServiceCollectionExtensions
{
    public static IServiceCollection AddManager(this IServiceCollection services)
    {
        services.AddScoped(typeof(DataAccessContext<>));
        services.AddScoped(typeof(ApiDocInfoManager));
        services.AddScoped(typeof(EntityInfoManager));
        services.AddScoped(typeof(FeatureManager));
        services.AddScoped(typeof(ProjectManager));
        services.AddScoped(typeof(ToolsManager));
        services.AddScoped(typeof(AdvanceManager));
        return services;
    }
}
