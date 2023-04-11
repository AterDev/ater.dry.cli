using ${Namespace}.IManager;
using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.Manager;

public class ${EntityName}Manager : DomainManagerBase<${EntityName}, ${EntityName}UpdateDto, ${EntityName}FilterDto, ${EntityName}ItemDto>, I${EntityName}Manager
{
${AdditionManagersProps}
    private readonly IUserContext _userContext;
    public ${EntityName}Manager(
        DataStoreContext storeContext, 
        IUserContext userContext${AdditionManagersDI}) : base(storeContext)
    {
${AdditionManagersInit}
        _userContext = userContext;
    }

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public Task<${EntityName}> CreateNewEntityAsync(${EntityName}AddDto dto)
    {
${AddActionBlock}
    }

    public override async Task<${EntityName}> UpdateAsync(${EntityName} entity, ${EntityName}UpdateDto dto)
    {
${UpdateActionBlock}
    }

    public override async Task<PageList<${EntityName}ItemDto>> FilterAsync(${EntityName}FilterDto filter)
    {
${FilterActionBlock}
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<${EntityName}?> GetOwnedAsync(${IdType} id)
    {
        var query = Command.Db.Where(q => q.Id == id);
        // TODO:获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
