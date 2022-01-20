using ${ShareNamespace}.Models.${EntityName}Dtos;
namespace ${Namespace}.DataStore;
public class ${EntityName}DataStore : DataStoreBase<${DbContextName}, ${EntityName}, ${EntityName}UpdateDto, ${EntityName}Filter, ${EntityName}ItemDto>
{
    public ${EntityName}DataStore(${DbContextName} context, IUserContext userContext, ILogger<${EntityName}DataStore> logger) : base(context, userContext, logger)
    {
    }
    public override Task<List<${EntityName}ItemDto>> FindAsync(${EntityName}Filter filter)
    {
        return base.FindAsync(filter);
    }

    public override Task<PageResult<${EntityName}ItemDto>> FindWithPageAsync(${EntityName}Filter filter)
    {
        return base.FindWithPageAsync(filter);
    }
    public override Task<${EntityName}> AddAsync(${EntityName} data) => base.AddAsync(data);
    public override Task<${EntityName}?> UpdateAsync(Guid id, ${EntityName}UpdateDto dto) => base.UpdateAsync(id, dto);
    public override Task<bool> DeleteAsync(Guid id) => base.DeleteAsync(id);
}
