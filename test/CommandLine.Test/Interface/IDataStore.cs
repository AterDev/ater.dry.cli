namespace CommandLine.Test.Interface;

public interface IDataStore<TEntity, TUpdate, TFilter, TItem, Tkey>
{
    /// <summary>
    /// 获取详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<TEntity?> FindAsync(Tkey id);
    /// <summary>
    /// 列表筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<List<TItem>> FindAsync(TFilter filter);
    /// <summary>
    /// 分页列表
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<PageResult<TItem>> FindWithPageAsync(TFilter filter);
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Tkey id);
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    Task<TEntity> AddAsync(TEntity form);
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<TEntity?> UpdateAsync(Tkey id, TUpdate dto);
    /// <summary>
    /// 判断实体是否存在
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    Task<bool> Exist(Tkey id);
    bool Any(Func<TEntity, bool> predicate);
}
/// <summary>
/// 默认提供guid 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TUpdate"></typeparam>
/// <typeparam name="TFilter"></typeparam>
/// <typeparam name="TItem"></typeparam>
public interface IDataStore<TEntity, TUpdate, TFilter, TItem> : IDataStore<TEntity, TUpdate, TFilter, TItem, Guid>
{
}


