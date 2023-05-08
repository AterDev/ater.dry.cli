using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ${Namespace}.Implement;

/// <summary>
/// 只读仓储基类,请勿直接修改基类内容
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public partial class QueryStoreBase<TContext, TEntity> :
    IQueryStore<TEntity>, IQueryStoreExt<TEntity>
    where TContext : DbContext
    where TEntity : EntityBase
{
    protected readonly ILogger _logger;
    /// <summary>
    /// 当前实体DbSet
    /// </summary>
    protected readonly DbSet<TEntity> _db;
    public DbSet<TEntity> Db => _db;
    public TContext Context { get; }
    public DatabaseFacade Database { get; init; }
    public IQueryable<TEntity> _query { get; set; }

    /// <summary>
    ///  是否开户全局筛选
    /// </summary>
    public bool EnableGlobalQuery { get; set; } = true;

    public QueryStoreBase(TContext context, ILogger logger)
    {
        Context = context;
        _logger = logger;
        _db = Context.Set<TEntity>();
        _query = EnableGlobalQuery
            ? _db.AsQueryable()
            : _db.IgnoreQueryFilters().AsQueryable();
        Database = Context.Database;
    }


    private void ResetQuery()
    {
        _query = EnableGlobalQuery
            ? _db.AsQueryable()
            : _db.IgnoreQueryFilters().AsQueryable();
    }

    public virtual async Task<TEntity?> FindAsync(Guid id)
    {
        TEntity? res = await _query.Where(d => d.Id == id)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        ResetQuery();
        return res;
    }

    public virtual async Task<TDto?> FindAsync<TDto>(Guid id)
        where TDto : class
    {
        TDto? res = await _query.Where(d => d.Id == id)
            .AsNoTracking()
            .ProjectTo<TDto>()
            .FirstOrDefaultAsync();
        ResetQuery();
        return res;
    }

    public virtual async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? whereExp)
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        TEntity? res = await _query.Where(whereExp)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        ResetQuery();
        return res;
    }

    /// <summary>
    /// 条件查询
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public virtual async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp)
        where TDto : class
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        TDto? res = await _query.Where(whereExp)
            .AsNoTracking()
            .ProjectTo<TDto>()
            .FirstOrDefaultAsync();
        ResetQuery();
        return res;
    }


    public virtual async Task<List<TEntity>> ListAsync(Expression<Func<TEntity, bool>>? whereExp)
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        List<TEntity> res = await _query.Where(whereExp)
            .AsNoTracking()
            .ToListAsync();
        ResetQuery();
        return res;
    }

    /// <summary>
    /// 列表条件查询
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="whereExp"></param>
    /// <returns></returns>
    public virtual async Task<List<TItem>> ListAsync<TItem>(Expression<Func<TEntity, bool>>? whereExp)
    {
        Expression<Func<TEntity, bool>> exp = e => true;
        whereExp ??= exp;
        List<TItem> res = await _query.Where(whereExp)
            .AsNoTracking()
            .ProjectTo<TItem>()
            .ToListAsync();
        ResetQuery();
        return res;
    }

    /// <summary>
    /// 分页筛选
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="query"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public virtual async Task<PageList<TItem>> PageListAsync<TItem>(IQueryable<TEntity> query, int pageIndex = 1, int pageSize = 12)
    {
        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        if (pageSize < 0)
        {
            pageSize = 12;
        }

        _query = query;

        int count = _query.Count();
        List<TItem> data = await _query
            .OrderByDescending(t => t.CreatedTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TItem>()
            .ToListAsync();

        ResetQuery();
        return new PageList<TItem>
        {
            Count = count,
            Data = data,
            PageIndex = pageIndex
        };
    }

    /// <summary>
    /// 分页筛选
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="query"></param>
    /// <param name="order"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public virtual async Task<PageList<TItem>> FilterAsync<TItem>(IQueryable<TEntity> query, int pageIndex = 1, int pageSize = 12, Dictionary<string, bool>? order = null)
    {
        if (pageIndex < 1)
        {
            pageIndex = 1;
        }

        if (query != null)
        {
            _query = query;
        }

        if (order != null)
        {
            _query = _query.OrderBy(order);
        }
        int count = _query.Count();
        List<TItem> data = await _query
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TItem>()
            .ToListAsync();
        ResetQuery();
        return new PageList<TItem>
        {
            Count = count,
            Data = data,
            PageIndex = pageIndex
        };
    }
}


public class QuerySet<TEntity> : QueryStoreBase<QueryDbContext, TEntity>
    where TEntity : EntityBase
{
    public QuerySet(QueryDbContext context, ILogger logger) : base(context, logger)
    {
    }
}

