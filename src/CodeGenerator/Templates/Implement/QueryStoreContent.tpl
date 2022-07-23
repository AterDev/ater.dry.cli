namespace ${Namespace}.DataStore;
public class ${EntityName}QueryDataStore : QuerySet<${EntityName}>
{
    public ${EntityName}QueryDataStore(${DbContextName} context, ILogger<${EntityName}QueryDataStore> logger) : base(context, logger)
    {
    }
}


