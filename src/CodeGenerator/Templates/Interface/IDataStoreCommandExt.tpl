namespace ${Namespace}.Interface;
public interface IDataStoreCommandExt<TId, TEntity>
    where TEntity : EntityBase
{
    /// <summary>
    /// 批量新增
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="chunk"></param>
    /// <returns></returns>
    Task<List<TEntity>> CreateRangeAsync(List<TEntity> entities, int? chunk = 50);

    /// <summary>
    /// 条件更新
    /// </summary>
    /// <typeparam name="TUpdate"></typeparam>
    /// <param name="whereExp"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<int> UpdateRangeAsync<TUpdate>(Expression<Func<TEntity, bool>> whereExp, TUpdate dto);

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <typeparam name="TUpdate"></typeparam>
    /// <param name="ids"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<int> EditRangeAsync<TUpdate>(List<TId> ids, TUpdate dto);

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    Task<int> DeleteRangeAsync(List<TId> ids);

    Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> whereExp);
}

public interface IDataStoreCommandExt<TEntity> : IDataStoreCommandExt<Guid, TEntity>
     where TEntity : EntityBase
{ }
