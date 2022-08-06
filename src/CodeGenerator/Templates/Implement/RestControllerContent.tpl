using ${ShareNamespace}.Models.${EntityName}Dtos;
namespace ${Namespace}.Infrastructure;

${Comment}
public class ${EntityName}${APISuffix} :
    RestControllerBase<${EntityName}Manager>,
    IRestController<${EntityName}, ${EntityName}AddDto, ${EntityName}UpdateDto, ${EntityName}FilterDto, ${EntityName}ItemDto>
{
    public ${EntityName}${APISuffix}(
        IUserContext user,
        ILogger<${EntityName}${APISuffix}> logger,
        ${EntityName}Manager manager
        ) : base(manager, user, logger)
    {
    }

    /// <summary>
    /// 筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<${EntityName}ItemDto>>> FilterAsync(${EntityName}FilterDto filter)
    {
        return await manager.FilterAsync<${EntityName}ItemDto>(filter);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<${EntityName}>> AddAsync(${EntityName}AddDto form)
    {
        var entity = form.MapTo<${EntityName}AddDto, ${EntityName}>();
        return await manager.AddAsync(entity);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="form"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<${EntityName}?>> UpdateAsync([FromRoute] Guid id, ${EntityName}UpdateDto form)
    {
        var current = await manager.GetCurrent(id);
        if (current == null) return NotFound();
        return await manager.UpdateAsync(current, form);
    }

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<${EntityName}?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await manager.FindAsync(id);
        return (res == null) ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<${EntityName}?>> DeleteAsync([FromRoute] Guid id)
    {
        return await manager.DeleteAsync(id);
    }
}