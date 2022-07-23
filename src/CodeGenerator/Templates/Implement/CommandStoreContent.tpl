namespace ${Namespace}.DataStore;
public class ${EntityName}CommandStore : CommandSet<${EntityName}>
{
    public ${EntityName}CommandStore(${DbContextName} context, ILogger<${EntityName}CommandStore> logger) : base(context, logger)
    {
    }

}
