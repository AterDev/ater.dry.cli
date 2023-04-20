using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.IManager;
/// <summary>
/// 定义实体业务接口规范
/// </summary>
public interface I${EntityName}Manager : IDomainManager<${EntityName}>
{
	/// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<${EntityName}?> GetOwnedAsync(${IdType} id);

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<${EntityName}> CreateNewEntityAsync(${EntityName}AddDto dto);

    /// <summary>
    /// 获取当前对象,通常是在修改前进行查询
    /// </summary>
    /// <param name="id"></param>
    /// <param name="navigations"></param>
    /// <returns></returns>
    Task<TEntity?> GetCurrentAsync(Guid id, params string[] navigations);
    Task<TEntity> AddAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity, TUpdate dto);
    Task<TEntity?> FindAsync(Guid id);
    /// <summary>
    /// 查询对象
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp) where TDto : class;
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
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<PageList<TItem>> FilterAsync(TFilter filter);
    Task<TEntity?> DeleteAsync(TEntity entity, bool softDelete = true);

    Task<bool> ExistAsync(Guid id);
}
