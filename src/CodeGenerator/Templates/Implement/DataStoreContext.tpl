namespace ${Namespace}.Implement;
public class DataStoreContext
{
    public QueryDbContext QueryContext { get; init; }
    public CommandDbContext CommandContext { get; init; }

${Properties}

    /// <summary>
    /// 绑在对象
    /// </summary>
    private readonly Dictionary<string, object> SetCache = new();

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
        var typename = typeof(TEntity).Name + "QueryDataStore";
        var set = GetSet(typename);
        if (set == null) throw new ArgumentNullException($"{typename} class object not found");
        return (QuerySet<TEntity>)set;
    }
    public CommandSet<TEntity> CommandSet<TEntity>() where TEntity : EntityBase
    {
        var typename = typeof(TEntity).Name + "CommandDataStore";
        var set = GetSet(typename);
        if (set == null) throw new ArgumentNullException($"{typename} class object not found");
        return (CommandSet<TEntity>)set;
    }

    private void AddCache(object set)
    {
        var typeName = set.GetType().Name;
        if (!SetCache.ContainsKey(typeName))
        {
            SetCache.Add(typeName, set);
        }
    }

    private object GetSet(string type)
    {
        return SetCache[type];
    }
}
