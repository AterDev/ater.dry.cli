namespace ${Namespace}.QueryStore;
public class ${EntityName}QueryStore : QuerySet<${EntityName}>
{
    public ${EntityName}QueryStore(${DbContextName} context, ILogger<${EntityName}QueryStore> logger) : base(context, logger)
    {
    }
}


