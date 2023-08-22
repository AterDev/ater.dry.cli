namespace ${Namespace};
public class DataStoreContext
{
    public QueryDbContext QueryContext { get; init; }
    public CommandDbContext CommandContext { get; init; }

${Properties}

    /// <summary>
    /// 字典缓存
    /// </summary>
    private readonly Dictionary<string, object> StoreCache = new();

    public DataStoreContext(
${CtorParams}
        QueryDbContext queryDbContext,
        CommandDbContext commandDbContext
    )
    {
        QueryContext = queryDbContext;
        CommandContext = commandDbContext;
${CtorAssign}
    }

    public async Task<int> SaveChangesAsync()
    {
        return await CommandContext.SaveChangesAsync();
    }

    public QuerySet<TEntity> QuerySet<TEntity>() where TEntity : EntityBase
    {
        var typename = typeof(TEntity).Name + "QueryStore";
        var set = GetSet(typename);
        return set == null 
            ? throw new ArgumentNullException($"{typename} class object not found") 
            : (QuerySet<TEntity>)set;
    }
    public CommandSet<TEntity> CommandSet<TEntity>() where TEntity : EntityBase
    {
        var typename = typeof(TEntity).Name + "CommandStore";
        var set = GetSet(typename);
        return set == null 
            ? throw new ArgumentNullException($"{typename} class object not found") 
            : (CommandSet<TEntity>)set;
    }

    private void AddCache(object set)
    {
        var typeName = set.GetType().Name;
        if (!StoreCache.ContainsKey(typeName))
        {
            StoreCache.Add(typeName, set);
        }
    }

    private object GetSet(string type)
    {
        return StoreCache[type];
    }
}
