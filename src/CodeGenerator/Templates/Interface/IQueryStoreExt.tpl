namespace ${Namespace}.Interface;
/// <summary>
/// 高级查询接口
/// </summary>
public interface IQueryStoreExt<TEntity>
{

    /// <summary>
    /// 条件查询
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="query"></param>
    /// <param name="order"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PageList<TItem>> FilterAsync<TItem>(IQueryable<TEntity> query, Dictionary<string, bool>? order, int pageIndex = 1, int pageSize = 12);
}
