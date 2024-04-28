using #@Namespace#.Manager;
using #@ShareNamespace#.Models.#@EntityName#Dtos;

namespace #@Namespace#.Manager;
#@Comment#
public class #@EntityName#Manager(
    DataAccessContext<#@EntityName#> dataContext, 
    ILogger<#@EntityName#Manager> logger,
    IUserContext userContext) : ManagerBase<#@EntityName#, #@EntityName#UpdateDto, #@EntityName#FilterDto, #@EntityName#ItemDto>(dataContext, logger)
{
    private readonly IUserContext _userContext = userContext;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<#@EntityName#> CreateNewEntityAsync(#@EntityName#AddDto dto)
    {
#@AddActionBlock#
    }

    public override async Task<#@EntityName#> UpdateAsync(#@EntityName# entity, #@EntityName#UpdateDto dto)
    {
#@UpdateActionBlock#
    }

    public override async Task<PageList<#@EntityName#ItemDto>> FilterAsync(#@EntityName#FilterDto filter)
    {
#@FilterActionBlock#
    }


    /// <summary>
    /// 是否唯一
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsConflictAsync(string unique)
    {
        // TODO:自定义唯一性验证参数和逻辑
        return await Command.Db.AnyAsync(q => q.Id == new Guid(unique));
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<#@EntityName#?> GetOwnedAsync(#@IdType# id)
    {
        var query = Command.Db.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
