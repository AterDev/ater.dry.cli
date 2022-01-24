namespace ${Namespace}.Interface;

public interface IRestApiBase<TEntity, TUpdate, TFilter, TItem, Tkey>
{
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    Task<ActionResult<TEntity>> AddAsync(TEntity form);
    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ActionResult<bool>> DeleteAsync(Tkey id);
    /// <summary>
    /// 分页筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<ActionResult<PageResult<TItem>>> FilterAsync(TFilter filter);
    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ActionResult<TEntity?>> GetDetailAsync(Tkey id);
    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="form"></param>
    /// <returns></returns>
    Task<ActionResult<TEntity?>> UpdateAsync(Tkey id, TUpdate form);
}
