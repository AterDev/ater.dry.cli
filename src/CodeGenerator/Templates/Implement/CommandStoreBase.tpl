using EFCore.BulkExtensions;

namespace ${Namespace}.Implement;
public class CommandStoreBase<TContext, TEntity> : IDataStoreCommand<TEntity>, IDataStoreCommandExt<TEntity>
    where TContext : DbContext
    where TEntity : EntityBase
{
    private readonly TContext _context;
    protected readonly ILogger _logger;
    /// <summary>
    /// 当前实体DbSet
    /// </summary>
    protected readonly DbSet<TEntity> _db;
    public bool EnableSoftDelete { get; set; } = true;

    //public TEntity CurrentEntity { get; }

    public CommandStoreBase(TContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
        _db = _context.Set<TEntity>();
    }

    public virtual async Task<int> SaveChangeAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? whereExp, string[]? navigations = null)
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        var _query = _db.Where(whereExp).AsQueryable();
        if (navigations != null)
        {
            foreach (var item in navigations)
            {
                _query = _query.Include(item);
            }
        }
        return await _query.FirstOrDefaultAsync();
    }


    /// <summary>
    /// 创建实体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        await _db.AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual TEntity Update(TEntity entity)
    {
        _db.Update(entity);
        return entity;
    }

    /// <summary>
    /// 删除实体,若未找到，返回null
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public virtual async Task<TEntity?> DeleteAsync(Guid id)
    {
        var entity = await _db.FindAsync(id);
        if (entity != null)
        {
            if (EnableSoftDelete)
            {
                entity.IsDeleted = true;
            }
            else
            {
                _db.Remove(entity!);
            }
        }
        return entity;
    }


    /// <summary>
    /// 批量创建
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="chunk"></param>
    /// <returns></returns>
    public virtual async Task<List<TEntity>> CreateRangeAsync(List<TEntity> entities, int? chunk = 50)
    {
        if (chunk != null && entities.Count > chunk)
        {
            entities.Chunk(chunk.Value).ToList()
                .ForEach(block =>
                {
                    _db.AddRange(block);
                    _context.SaveChanges();
                });
        }
        else
        {
            await _db.AddRangeAsync(entities);
            await SaveChangeAsync();
        }
        return entities;
    }

    /// <summary>
    /// 条件更新
    /// </summary>
    /// <typeparam name="TUpdate"></typeparam>
    /// <param name="whereExp"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public virtual async Task<int> UpdateRangeAsync<TUpdate>(Expression<Func<TEntity, bool>> whereExp, TUpdate dto)
    {
        return await _db.Where(whereExp).BatchUpdateAsync(dto!);
    }

    /// <summary>
    /// 批量编辑
    /// </summary>
    /// <typeparam name="TUpdate"></typeparam>
    /// <param name="ids"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public virtual async Task<int> EditRangeAsync<TUpdate>(List<Guid> ids, TUpdate dto)
    {
        return await _db.Where(d => ids.Contains(d.Id)).BatchUpdateAsync(dto!);
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public virtual async Task<int> DeleteRangeAsync(List<Guid> ids)
    {
        return await _db.Where(d => ids.Contains(d.Id)).BatchDeleteAsync();
    }

    /// <summary>
    /// 条件删除
    /// </summary>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public virtual async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> whereExp)
    {
        return await _db.Where(whereExp).BatchDeleteAsync();
    }
}
public class CommandSet<TEntity> : CommandStoreBase<CommandDbContext, TEntity>
    where TEntity : EntityBase
{
    public CommandSet(CommandDbContext context, ILogger logger) : base(context, logger)
    {
    }
}