namespace ${Namespace}.Interface;
/// <summary>
/// 基础查询接口
/// </summary>
public interface IQueryStore<TId, TEntity>
    where TEntity : EntityBase
{
    /// <summary>
    /// id查询 
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TDto?> FindAsync<TDto>(TId id)
        where TDto : class;
    Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class;

    /// <summary>
    /// 列表条件查询
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    Task<List<TItem>> ListAsync<TItem>(Expression<Func<TEntity, bool>>? whereExp);

    /// <summary>
    /// 分页查询
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="whereExp"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PageList<TItem>> PageListAsync<TItem>(Expression<Func<TEntity, bool>>? whereExp, int pageIndex = 1, int pageSize = 12);
}

public interface IQueryStore<TEntity> : IQueryStore<Guid, TEntity>
    where TEntity : EntityBase
{ }