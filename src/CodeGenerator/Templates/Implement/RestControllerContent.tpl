using ${ShareNamespace}.Models.${EntityName}Dtos;
namespace ${Namespace}.Controllers;

${Comment}
public class ${EntityName}${APISuffix} : RestControllerBase<I${EntityName}Manager>
{
    public ${EntityName}${APISuffix}(
        IUserContext user,
        ILogger<${EntityName}${APISuffix}> logger,
        I${EntityName}Manager manager
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
        return await manager.FilterAsync(filter);
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
    public async Task<ActionResult<${EntityName}?>> UpdateAsync([FromRoute] ${IdType} id, ${EntityName}UpdateDto form)
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
    public async Task<ActionResult<${EntityName}?>> GetDetailAsync([FromRoute] ${IdType} id)
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
    public async Task<ActionResult<${EntityName}?>> DeleteAsync([FromRoute] ${IdType} id)
    {
        var entity = await manager.GetCurrent(id);
        if (entity == null) return NotFound();
        return await manager.DeleteAsync(entity);
    }
}