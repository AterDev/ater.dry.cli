using ${Namespace}.IManager;
using Share.Models.${EntityName}Dtos;

namespace ${Namespace}.Manager;

public class ${EntityName}Manager : DomainManagerBase<${EntityName}, ${EntityName}UpdateDto, ${EntityName}FilterDto>, I${EntityName}Manager
{
    public ${EntityName}Manager(DataStoreContext storeContext) : base(storeContext)
    {
    }

    public override async Task<${EntityName}> UpdateAsync(${EntityName} entity, ${EntityName}UpdateDto dto)
    {
        // TODO:根据实际业务更新
        return await base.UpdateAsync(entity, dto);
    }

    public override Task<PageList<TItem>> FilterAsync<TItem>(${EntityName}FilterDto filter)
    {
        // TODO:根据实际业务构建筛选条件
        return await  base.FilterAsync<TItem>(filter);
    }

}
