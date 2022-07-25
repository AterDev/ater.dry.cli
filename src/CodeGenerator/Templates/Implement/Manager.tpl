using ${Namespace}.IManager;
using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.Manager;

public class ${EntityName}Manager : DomainManagerBase<${EntityName}, ${EntityName}UpdateDto>, I${EntityName}Manager
{
    public ${EntityName}Manager(DataStoreContext storeContext) : base(storeContext)
    {
    }

    public override async Task<${EntityName}> UpdateAsync(${EntityName} entity, ${EntityName}UpdateDto dto)
    {
        // TODO:根据实际业务更新
        return await base.UpdateAsync(entity, dto);
    }

    public override Task<PageList<TItem>> FilterAsync<TItem, TFilter>(TFilter filter)
    {
        // TODO:根据实际业务构建筛选条件
        Expression<Func<${EntityName}, bool>> exp = e => true;
        return Query.FilterAsync<TItem>(exp, filter.OrderBy, filter.PageIndex ?? 1, filter.PageSize ?? 12);
    }

}
