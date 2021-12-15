
namespace Core.Services.Repositories
{

    public class {$EntityName}Repository : Repository<{$EntityName}, {$EntityName}AddDto, {$EntityName}UpdateDto, {$EntityName}Filter, {$EntityName}Dto>
    {

        ILogger _logger;
        public {$EntityName}Repository({$ContextName} context, ILogger<{$EntityName}Repository> logger, IUserContext userContext, IMapper mapper) 
        : base(context, logger, userContext, mapper)
        {

        }

        public override Task<PageResult<{$EntityName}Dto>> GetListWithPageAsync({$EntityName}Filter filter)
        {
            _query = _query.OrderByDescending(q => q.CreatedTime);
            return base.GetListWithPageAsync(filter);
        }

        public override Task<{$EntityName}> AddAsync({$EntityName}AddDto form)
        {
            return base.AddAsync(form);
        }

        public override Task<{$EntityName}> UpdateAsync(Guid id, {$EntityName}UpdateDto form)
        {
            return base.UpdateAsync(id, form);
        }
        public override Task<{$EntityName}> DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }
        public override Task<{$EntityName}> GetDetailAsync(Guid id)
        {
            return base.GetDetailAsync(id);
        }





    }
}
