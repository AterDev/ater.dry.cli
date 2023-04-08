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

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public Task<${EntityName}> CreateNewEntityAsync(${EntityName}AddDto dto)
    {
        var entity = dto.MapTo<${EntityName}AddDto, ${EntityName}>();
        // 构建实体
        return Task.FromResult(entity);
    }

    public override async Task<${EntityName}> UpdateAsync(${EntityName} entity, ${EntityName}UpdateDto dto)
    {
        // 根据实际业务更新
        return await base.UpdateAsync(entity, dto);
    }

    public override async Task<PageList<${EntityName}ItemDto>> FilterAsync(${EntityName}FilterDto filter)
    {
        // TODO:根据实际业务构建筛选条件
        // example: Queryable = Queryable.WhereNotNull(filter.field, q => q.field = filter.field);
        return await Query.FilterAsync<${EntityName}ItemDto>(Queryable, filter.PageIndex, filter.PageSize);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<${EntityName}?> GetOwnedAsync(Guid id)
    {
        var query = Command.Db.Where(q => q.Id == id);
        // TODO:获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }
}
