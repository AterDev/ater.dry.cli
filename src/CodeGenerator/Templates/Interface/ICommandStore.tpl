namespace ${Namespace}.Interface;

/// <summary>
/// 仓储命令
/// </summary>
public interface ICommandStore<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 创建模型
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<TEntity> CreateAsync(TEntity entity);

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Update(TEntity entity);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity? Remove(TEntity entity);
}
