using ${Namespace}.IManager;
using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.Manager;

public class ${EntityName}Manager : DomainManagerBase<${EntityName}, ${EntityName}UpdateDto, ${EntityName}FilterDto, ${EntityName}ItemDto>, I${EntityName}Manager
{

    private readonly IUserContext _userContext;
    public ${EntityName}Manager(DataStoreContext storeContext, IUserContext userContext) : base(storeContext)
    {
        _userContext = userContext;
    }

    public override async Task<${EntityName}> UpdateAsync(${EntityName} entity, ${EntityName}UpdateDto dto)
    {
        // TODO:根据实际业务更新
        return await base.UpdateAsync(entity, dto);
    }

    public override async Task<PageList<${EntityName}ItemDto>> FilterAsync(${EntityName}FilterDto filter)
    {
        // TODO:根据实际业务构建筛选条件
        // if ... Queryable = ...
        return await Query.FilterAsync<${EntityName}ItemDto>(Queryable, filter.PageIndex, filter.PageSize);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<${EntityName}?> GetOwnedAsync(Guid id)
    {
        Queryable = Queryable.Where(q => q.Id == id);
        if (!_userContext.IsAdmin)
        {
            // TODO:属于当前角色的对象
            // Queryable = Queryable.Where(q => q.User.Id == _userContext.UserId);
        }
        return await Queryable.FirstOrDefaultAsync();

    }
}
