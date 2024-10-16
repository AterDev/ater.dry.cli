﻿using #@ShareNamespace#.Models.#@EntityName#Dtos;
namespace #@Namespace#.Controllers;

#@Comment#
public class #@EntityName##@APISuffix#(
//[@AdditionManagersDI]
    IUserContext user,
    ILogger<#@EntityName##@APISuffix#> logger,
    #@EntityName#Manager manager
    ) : RestControllerBase<#@EntityName#Manager>(manager, user, logger)
{
//[@AdditionManagersProps]
    /// <summary>
    /// 筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<#@EntityName#ItemDto>>> FilterAsync(#@EntityName#FilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<#@EntityName#>> AddAsync(#@EntityName#AddDto dto)
    {
//[@AddActionBlock]
    }

    /// <summary>
    /// 部分更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<#@EntityName#?>> UpdateAsync([FromRoute] #@IdType# id, #@EntityName#UpdateDto dto)
    {
//[@UpdateActionBlock]
    }

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<#@EntityName#?>> GetDetailAsync([FromRoute] #@IdType# id)
    {
        var res = await _manager.FindAsync(id);
        return (res == null) ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<#@EntityName#?>> DeleteAsync([FromRoute] #@IdType# id)
    {
        // 注意删除权限
        var entity = await _manager.GetOwnedAsync(id);
        if (entity == null) { return NotFound(); };
        // return Forbid();
        return await _manager.DeleteAsync(entity);
    }
}