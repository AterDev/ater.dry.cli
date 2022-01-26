namespace ${Namespace}.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RestApiBase<TDataStore, TEntity, TUpdate, TFilter, TItem>
    : ControllerBase, IRestApiBase<TEntity, TUpdate, TFilter, TItem, Guid>
    where TDataStore : DataStoreBase<${DbContextName}, TEntity, TUpdate, TFilter, TItem>
    where TEntity : BaseDB
    where TFilter : FilterBase
{
    protected readonly ILogger _logger;
    protected readonly TDataStore _store;
    protected readonly IUserContext _user;

    public RestApiBase(IUserContext user, ILogger logger, TDataStore store)
    {
        _user = user;
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPost]
    public async virtual Task<ActionResult<TEntity>> AddAsync(TEntity form)
        => await _store.AddAsync(form);

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async virtual Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        if (_store.Any(d => d.Id == id))
        {
            return await _store.DeleteAsync(id);
        }
        return NotFound();
    }

    /// <summary>
    /// 分页筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async virtual Task<ActionResult<PageResult<TItem>>> FilterAsync(TFilter filter)
        => await _store.FindWithPageAsync(filter);

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async virtual Task<ActionResult<TEntity?>> GetDetailAsync([FromRoute] Guid id)
    {
        var data = await _store.FindAsync(id);
        if (data == null) return NotFound();
        return data;
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async virtual Task<ActionResult<TEntity?>> UpdateAsync([FromRoute] Guid id, TUpdate form)
    {
        if (_store.Any(d => d.Id == id))
        {
            return await _store.UpdateAsync(id, form);
        }
        return NotFound();
    }

    /// <summary>
    /// 批量删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete]
    public async virtual Task<ActionResult<int>> BatchDeleteAsync(List<Guid> ids)
        => await _store.BatchDeleteAsync(ids);

    /// <summary>
    /// 批量更新
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    [HttpPut]
    public async virtual Task<int> BatchUpdateAsync([FromBody] BatchUpdate<TUpdate> data)
    => await _store.BatchUpdateAsync(data.Ids, data.UpdateDto);
}
