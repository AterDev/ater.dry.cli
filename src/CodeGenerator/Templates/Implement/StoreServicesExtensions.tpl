namespace ${Namespace}.Infrastructure;

public static partial class StoreServicesExtensions
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
