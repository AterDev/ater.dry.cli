namespace ${Namespace}.Interface;

/// <summary>
/// 仓储数据管理接口
/// </summary>
public interface IDomainManager<TEntity, TUpdate, TFilter，TItem>
       where TEntity : EntityBase
       where TFilter : FilterBase
{
    DataStoreContext Stores { get; init; }
    QuerySet<TEntity> Query { get; init; }
    CommandSet<TEntity> Command { get; init; }

    /// <summary>
    /// 获取当前对象,通常是在修改前进行查询
    /// </summary>
    /// <param name="id"></param>
    /// <param name="navigations">include </param>
    /// <returns></returns>
    Task<TEntity?> GetCurrent(Guid id, params string[] navigations);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto);
    Task<TEntity?> DeleteAsync(TEntity entity);
    Task<TEntity?> FindAsync(Guid id);
    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TDto?> FindAsync<TDto>(Guid id) where TDto : class;


    /// <summary>
    /// 列表条件查询
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    Task<List<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class;

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <typeparam name="TFilter"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<PageList<TItem>> FilterAsync(TFilter filter);
}
