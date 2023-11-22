using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Implement;

public partial class DomainManagerBase<TEntity, TUpdate, TFilter, TItem> : ManagerBase<TEntity, TUpdate, TFilter, TItem>
    where TEntity : class ,IEntityBase
    where TFilter : FilterBase
{
    protected IUserContext? _userContext;
    protected readonly ILogger? _logger;

    public DomainManagerBase(DataStoreContext storeContext) : base(storeContext)
    {
    }
    public DomainManagerBase(DataStoreContext storeContext, ILogger logger) : base(storeContext)
    {
         _logger = logger;
    }

    public DomainManagerBase(DataStoreContext storeContext, IUserContext userContext, ILogger logger) : base(storeContext)
    {
        _userContext = userContext;
		_logger = logger;
    }
}