using ${Namespace}.IManager;
using ${ShareNamespace}.Models.${EntityName}Dtos;

namespace ${Namespace}.Manager;
${Comment}
public class ${EntityName}Manager : DomainManagerBase<${EntityName}, ${EntityName}UpdateDto, ${EntityName}FilterDto, ${EntityName}ItemDto>, IDomainManager<${EntityName}>
{
${AdditionManagersProps}
    public ${EntityName}Manager(
        DataStoreContext storeContext, 
        ILogger<${EntityName}Manager> logger,
        IUserContext userContext${AdditionManagersDI}) : base(storeContext, logger)
    {
${AdditionManagersInit}
        _userContext = userContext;
    }

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<${EntityName}> CreateNewEntityAsync(${EntityName}AddDto dto)
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
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

}
