namespace ${Namespace}.DataStore;

public static class DataStoreExtensions
{
    public static void AddDataStore(this IServiceCollection services)
    {
        services.AddTransient<IUserContext, UserContext>();
        services.AddScope(typeof(DataStoreContext));
//${DataStoreServices}
    }
}
