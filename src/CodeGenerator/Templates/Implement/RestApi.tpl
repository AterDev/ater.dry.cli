using ${ShareNamespace}.Models.${EntityName}Dtos;

namespace ${Namespace}.Controllers;

/// <summary>
/// ${Comment}
/// </summary>
public class ${EntityName}Controller : RestApiBase<${EntityName}DataStore, ${EntityName}, ${EntityName}UpdateDto, ${EntityName}Filter, ${EntityName}ItemDto>
{
    public ${EntityName}Controller(IUserContext user, ILogger<${EntityName}Controller> logger, ${EntityName}DataStore store) : base(user, logger, store)
    {
    }

    public override Task<ActionResult<PageResult<${EntityName}ItemDto>>> FilterAsync(${EntityName}Filter filter)
    {
        return base.FilterAsync(filter);
    }

    public override Task<ActionResult<${EntityName}>> AddAsync(${EntityName} form)
    {
        return base.AddAsync(form);
    }

    public override Task<ActionResult<${EntityName}?>> UpdateAsync([FromRoute] Guid id, ${EntityName}UpdateDto form)
        => base.UpdateAsync(id, form);

    /// <summary>
    /// ⚠删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        return base.DeleteAsync(id);
    }

    /// <summary>
    /// ⚠ 批量删除
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public override async Task<ActionResult<int>> BatchDeleteAsync(List<Guid> ids)
    {
        // 危险操作，请确保该方法的执行权限
        //return base.BatchDeleteAsync(ids);
        return await Task.FromResult(0);
    }
}
