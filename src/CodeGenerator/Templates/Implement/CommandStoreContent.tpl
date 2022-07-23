namespace ${Namespace}.CommandStore;
public class ${EntityName}CommandStore : CommandSet<${EntityName}>
{
    public ${EntityName}CommandStore(${DbContextName} context, ILogger<${EntityName}CommandStore> logger) : base(context, logger)
    {
    }

}
