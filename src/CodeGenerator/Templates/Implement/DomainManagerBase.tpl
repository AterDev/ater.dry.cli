namespace ${Namespace}.Implement;

public class DomainManagerBase<TEntity, TUpdate> : IDomainManager<TEntity, TUpdate>
    where TEntity : EntityBase
{
    public DataStoreContext Stores { get; init; }
    public QuerySet<TEntity> Query { get; init; }
    public CommandSet<TEntity> Command { get; init; }

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
        return await Command.CreateAsync(entity);
    }

    public virtual Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto)
    {
        entity.Merge(dto);
        var res = Command.Update(entity);
        return Task.FromResult(res);
    }

    public virtual async Task<TEntity?> DeleteAsync(Guid id)
    {
        return await Command.DeleteAsync(id);
    }

    public virtual async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class
    {
        return await Query.FindAsync<TDto>(whereExp);
    }

    /// <summary>
    /// 分页筛选，需要重写该方法
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TFilter"></typeparam>
    /// <param name="filter"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public virtual async Task<PageList<TItem>> FilterAsync<TItem, TFilter>(TFilter filter)
        where TFilter : FilterBase
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        return await Query.FilterAsync<TItem>(exp, filter.OrderBy, filter.PageIndex ?? 1, filter.PageSize ?? 12);
    }

}
