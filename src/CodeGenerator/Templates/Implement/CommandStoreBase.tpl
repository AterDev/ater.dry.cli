using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ${Namespace}.Implement;
/// <summary>
/// 读写仓储基类,请勿直接修改基类内容 
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public partial class CommandStoreBase<TContext, TEntity> : ICommandStore<TEntity>, ICommandStoreExt<TEntity>
    where TContext : DbContext
    where TEntity : EntityBase
{
    protected readonly ILogger _logger;
    /// <summary>
    /// 当前实体DbSet
    /// </summary>
    protected readonly DbSet<TEntity> _db;
    public DbSet<TEntity> Db => _db;
    /// <summary>
    /// use DataStoreContext.CommandContext to access writable DbContext
    /// this will be not avaliable in the future
    /// </summary>
    protected TContext Context { get; }
    public DatabaseFacade Database { get; init; }
    public bool EnableSoftDelete { get; set; } = true;

    //public TEntity CurrentEntity { get; }

    public CommandStoreBase(TContext context, ILogger logger)
    {
        Context = context;
        _logger = logger;
        _db = Context.Set<TEntity>();
        Database = Context.Database;
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }

    public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? whereExp, string[]? navigations = null)
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        IQueryable<TEntity> _query = _db.Where(whereExp).AsQueryable();
        if (navigations != null)
        {
            foreach (string item in navigations)
            {
                _query = _query.Include(item);
            }
        }
        return await _query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 列表条件查询
    /// </summary>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public virtual async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? whereExp = null)
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        List<TEntity> res = await _db.Where(whereExp)
            .ToListAsync();
        return res;
    }

    /// <summary>
    /// 创建实体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        _ = await _db.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual TEntity Update(TEntity entity)
    {
        _ = _db.Update(entity);
        return entity;
    }

    /// <summary>
    /// 删除实体,若未找到，返回null
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual TEntity? Remove(TEntity entity)
    {
        if (EnableSoftDelete)
        {
            entity.IsDeleted = true;
        }
        else
        {
            _ = _db.Remove(entity!);
        }
        return entity;
    }
    /// <summary>
    /// 移除实体
    /// </summary>
    /// <param name="entities"></param>
    public virtual void RemoveRange(List<TEntity> entities)
    {
        if (EnableSoftDelete)
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
            }
        }
        else
        {
            _db.RemoveRange(entities);
        }
    }

    /// <summary>
    /// 批量创建
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="chunk">每个批次的最大数量</param>
    /// <returns></returns>
    public virtual async Task<List<TEntity>> CreateRangeAsync(List<TEntity> entities, int? chunk = 50)
    {
        if (chunk != null && entities.Count > chunk)
        {

            entities.Chunk(entities.Count / chunk.Value + 1).ToList()
                .ForEach(block =>
                {
                    _db.AddRange(block);
                    _ = Context.SaveChanges();
                });
        }
        else
        {
            await _db.AddRangeAsync(entities);
            _ = await SaveChangesAsync();
        }
        return entities;
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public virtual async Task<int> DeleteRangeAsync(List<${IdType}> ids)
    {
        return await _db.Where(d => ids.Contains(d.Id)).ExecuteDeleteAsync();
    }

    /// <summary>
    /// 条件删除
    /// </summary>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public virtual async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> whereExp)
    {
        return await _db.Where(whereExp).ExecuteDeleteAsync();
    }
}
public class CommandSet<TEntity> : CommandStoreBase<CommandDbContext, TEntity>
    where TEntity : EntityBase
{
    public CommandSet(CommandDbContext context, ILogger logger) : base(context, logger)
    {
    }
}