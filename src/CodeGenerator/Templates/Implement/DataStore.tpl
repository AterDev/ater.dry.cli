﻿using ${ShareNamespace}.Models.${EntityName}Dtos;
namespace ${Namespace}.DataStore;
public class ${EntityName}DataStore : DataStoreBase<${DbContextName}, ${EntityName}, ${EntityName}UpdateDto, ${EntityName}Filter, ${EntityName}ItemDto>
{
    public ${EntityName}DataStore(${DbContextName} context, IUserContext userContext, ILogger<${EntityName}DataStore> logger) : base(context, userContext, logger)
    {
    }
    public override async Task<List<${EntityName}ItemDto>> FindAsync(${EntityName}Filter filter)
    {
        return await base.FindAsync(filter);
    }

    public override async Task<PageResult<${EntityName}ItemDto>> FindWithPageAsync(${EntityName}Filter filter)
    {
        return await base.FindWithPageAsync(filter);
    }
    public override async Task<${EntityName}> AddAsync(${EntityName} data) => await base.AddAsync(data);
    public override async Task<${EntityName}?> UpdateAsync(Guid id, ${EntityName}UpdateDto dto) => await base.UpdateAsync(id, dto);
    public override async Task<bool> DeleteAsync(Guid id) => await base.DeleteAsync(id);
}
