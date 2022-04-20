using ${ShareNamespace}.Models.${EntityName}Dtos;
namespace ${Namespace}.Controllers;

${Comment}
public class ${EntityName}Controller : RestApiBase<${EntityName}DataStore, ${EntityName}, ${EntityName}AddDto, ${EntityName}UpdateDto, ${EntityName}Filter, ${EntityName}ItemDto>
{
    public ${EntityName}Controller(IUserContext user, ILogger<${EntityName}Controller> logger, ${EntityName}DataStore store) : base(user, logger, store)
    {
    }
${AdditionAction}
    /// <summary>
    /// 分页筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public override async Task<ActionResult<PageResult<${EntityName}ItemDto>>> FilterAsync(${EntityName}Filter filter)
    {
        return await base.FilterAsync(filter);
    }

    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public override async Task<ActionResult<${EntityName}>> AddAsync(${EntityName}AddDto form) => await base.AddAsync(form);

    /// <summary>
    /// ⚠更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="form"></param>
    /// <returns></returns>
    public override async Task<ActionResult<${EntityName}?>> UpdateAsync([FromRoute] ${IdType} id, ${EntityName}UpdateDto form)
        => await base.UpdateAsync(id, form);

    /// <summary>
    /// ⚠删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    public override async Task<ActionResult<bool>> DeleteAsync([FromRoute] ${IdType} id)
    {
        return await base.DeleteAsync(id);
    }

    /// <summary>
    /// ⚠ 批量删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public override async Task<ActionResult<int>> BatchDeleteAsync(List<${IdType}> ids)
    {
        // 危险操作，请确保该方法的执行权限
        //return await base.BatchDeleteAsync(ids);
        return await Task.FromResult(0);
    }
}
