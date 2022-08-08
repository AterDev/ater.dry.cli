namespace ${Namespace}.Implement;

public class DomainManagerBase<TEntity, TUpdate, TFilter> : IDomainManager<TEntity, TUpdate, TFilter>
    where TEntity : EntityBase
    where TFilter : FilterBase
{
    public DataStoreContext Stores { get; init; }
    public QuerySet<TEntity> Query { get; init; }
    public CommandSet<TEntity> Command { get; init; }

    /// <summary>
    /// 是否自动保存(调用SaveChanges)
    /// </summary>
    public bool AutoSave { get; set; } = true;
    public DomainManagerBase(DataStoreContext storeContext)
    {
        Stores = storeContext;
        Query = Stores.QuerySet<TEntity>();
        Command = Stores.CommandSet<TEntity>();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Stores.SaveChangesAsync();
    }

    public async Task AutoSaveAsync()
    {
        if (AutoSave)
        {
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// 在修改前查询对象
    /// </summary>
    /// <param name="id"></param>
    /// <param name="navigations">include navigations</param>
    /// <returns></returns>
    public virtual async Task<TEntity?> GetCurrent(Guid id, params string[]? navigations)
    {
        return await Command.FindAsync(e => e.Id == id, navigations);
    }
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var res = await Command.CreateAsync(entity);
        await AutoSaveAsync();
        return res;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto)
    {
        entity.Merge(dto);
        var res = Command.Update(entity);
        await AutoSaveAsync();
        return res;
    }

    public virtual async Task<TEntity?> DeleteAsync(Guid id)
    {
        var res = await Command.DeleteAsync(id);
        await AutoSaveAsync();
        return res;
    }

    public virtual async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class
    {
        return await Query.FindAsync<TDto>(whereExp);
    }

    /// <summary>
    /// 条件查询列表
    /// </summary>
    /// <typeparam name="TDto">返回类型</typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public async Task<List<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class
    {
        return await Query.ListAsync<TDto>(whereExp);
    }

    /// <summary>
    /// 获取当前查询构造对象
    /// </summary>
    /// <returns></returns>
    public IQueryable<TEntity> GetQueryable()
    {
        return Query._query;
    }

    /// <summary>
    /// 分页筛选，重写该方法实现自己的查询逻辑
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    public virtual async Task<PageList<TItem>> FilterAsync<TItem>(TFilter filter)
    {
        return await Query.FilterAsync<TItem>(GetQueryable(), filter.OrderBy, filter.PageIndex ?? 1, filter.PageSize ?? 12);
    }

}