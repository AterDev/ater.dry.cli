namespace ${Namespace}.Implement;

public static partial class StoreServicesExtensions
{
    public static void AddDataStore(this IServiceCollection services)
    {
        services.AddScoped(typeof(DataStoreContext));
${StoreServices}
    }

    public static void AddManager(this IServiceCollection services)
    {
        services.AddTransient<IUserContext, UserContext>();
${ManagerServices}
    }
}
