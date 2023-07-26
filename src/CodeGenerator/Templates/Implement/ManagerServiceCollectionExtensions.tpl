// 本文件由ater.droplet.cli工具自动生成.
namespace ${Namespace}.Infrastructure;

public static partial class ManagerServiceCollectionExtensions
{
    public static void AddDataStore(this IServiceCollection services)
    {
        services.AddScoped(typeof(DataStoreContext));
${StoreServices}
    }

    public static void AddManager(this IServiceCollection services)
    {
${ManagerServices}
    }
}
