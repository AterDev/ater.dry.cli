using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Implement;

public class DomainManagerBase<TEntity, TUpdate, TFilter, TItem> : IDomainManager<TEntity, TUpdate, TFilter, TItem>
    where TEntity : EntityBase
    where TFilter : FilterBase
{
    /// <summary>
    /// 仓储上下文，可通过Store访问到其他实体的上下文
    /// </summary>
    public DataStoreContext Stores { get; init; }
    /// <summary>
    /// 实体的只读仓储实现
    /// </summary>
    public QuerySet<TEntity> Query { get; init; }
    /// <summary>
    /// 实体的可写仓储实现
    /// </summary>
    public CommandSet<TEntity> Command { get; init; }
    public IQueryable<TEntity> Queryable { get; set; }
    /// <summary>
    /// 是否自动保存(调用SaveChanges)
    /// </summary>
    public bool AutoSave { get; set; } = true;
    public DatabaseFacade Database { get; init; }
    public DomainManagerBase(DataStoreContext storeContext)
    {
        Stores = storeContext;
        Query = Stores.QuerySet<TEntity>();
        Command = Stores.CommandSet<TEntity>();
        Queryable = Query._query;
        Database = Command.Database;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await Stores.SaveChangesAsync();
    }

    private async Task AutoSaveAsync()
    {
        if (AutoSave)
        {
            _ = await SaveChangesAsync();
        }
    }

    /// <summary>
    /// 在修改前查询对象
    /// </summary>
    /// <param name="id"></param>
    /// <param name="navigations">include navigations</param>
    /// <returns></returns>
    public virtual async Task<TEntity?> GetCurrentAsync(Guid id, params string[]? navigations)
    {
        return await Command.FindAsync(e => e.Id == id, navigations);
    }
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        TEntity res = await Command.CreateAsync(entity);
        await AutoSaveAsync();
        return res;
    }

    [Obsolete("use GetCurrentAsync")]
    public virtual async Task<TEntity?> GetCurrent(Guid id, params string[]? navigations)
    {
        return await Command.FindAsync(e => e.Id == id, navigations);
    }


    public virtual async Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto)
    {
        _ = entity.Merge(dto, false);
        entity.UpdatedTime = DateTimeOffset.UtcNow;
        TEntity res = Command.Update(entity);
        await AutoSaveAsync();
        return res;
    }

    public virtual async Task<TEntity?> DeleteAsync(TEntity entity, bool softDelete = true)
    {
        Command.EnableSoftDelete = softDelete;
        TEntity? res = Command.Remove(entity);
        await AutoSaveAsync();
        return res;
    }

    public virtual async Task<TEntity?> FindAsync(Guid id)
    {
        return await Query.FindAsync(q => q.Id == id);
    }

    public async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class
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
    public async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? whereExp)
    {
        return await Query.ListAsync(whereExp);
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
    /// 分页筛选，需要重写该方法
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public virtual async Task<PageList<TItem>> FilterAsync(TFilter filter)
    {
        return await Query.FilterAsync<TItem>(Queryable, filter.PageIndex, filter.PageSize, filter.OrderBy);
    }

}
