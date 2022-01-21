namespace CommandLine.Test.DataStore;

public static class DataStoreExtensions
{
    public static void AddDataStore(this IServiceCollection services)
    {
        services.AddScoped(typeof(BlogDataStore));

    }
}
