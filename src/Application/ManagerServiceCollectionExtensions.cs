// 本文件由 ater.dry工具自动生成.
namespace Application;

public static partial class ManagerServiceCollectionExtensions
{
    public static void AddManager(this IServiceCollection services)
    {
        services.AddScoped(typeof(DataAccessContext<>));
        services.AddScoped(typeof(ApiDocInfoManager));
        services.AddScoped(typeof(EntityManager));
        services.AddScoped(typeof(FeatureManager));
        services.AddScoped(typeof(ProjectManager));
        services.AddScoped(typeof(SwaggerManager));
        services.AddScoped(typeof(ToolsManager));

    }
}
