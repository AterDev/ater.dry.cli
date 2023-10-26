using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Implement;

/// <summary>
/// 基类，请勿直接修改，自定义可修改 DomainManagerBase
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TUpdate"></typeparam>
/// <typeparam name="TFilter"></typeparam>
/// <typeparam name="TItem"></typeparam>
public partial class ManagerBase<TEntity, TUpdate, TFilter, TItem>
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
    protected CommandDbContext CommandContext { get; set; }
    protected QueryDbContext QueryContext { get; set; }

    /// <summary>
    /// 是否自动保存(调用SaveChanges)
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// Command database
    /// </summary>
    public DatabaseFacade Database { get; init; }

    public ManagerBase(DataStoreContext storeContext)
    {
        Stores = storeContext;
        Query = Stores.QuerySet<TEntity>();
        Command = Stores.CommandSet<TEntity>();
        CommandContext = Stores.CommandContext;
        QueryDbContext = Stores.QueryContext;
        Queryable = Query.Queryable;
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
    public virtual async Task<TEntity?> GetCurrentAsync(${IdType} id, params string[]? navigations)
    {
        return await Command.FindAsync(e => e.Id == id, navigations);
    }
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        TEntity res = await Command.CreateAsync(entity);
        await AutoSaveAsync();
        return res;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto, bool ignoreNull = true)
    {
        _ = entity.Merge(dto, ignoreNull);
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

    public virtual async Task<TEntity?> FindAsync(${IdType} id)
    {
        return await Query.FindAsync(q => q.Id == id);
    }

    public virtual async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null) where TDto : class
    {
        return await Query.FindAsync<TDto>(whereExp);
    }
    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="id">主键id</param>
    /// <returns></returns>
    public virtual async Task<bool> ExistAsync(${IdType} id)
    {
        return await Query.Db.AnyAsync(q => q.Id == id);
    }

    /// <summary>
    /// 条件查询列表
    /// </summary>
    /// <typeparam name="TDto">返回类型</typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public virtual async Task<List<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null) where TDto : class
    {
        return await Query.ListAsync<TDto>(whereExp);
    }
    public virtual async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? whereExp = null)
    {
        return await Query.ListAsync(whereExp);
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
