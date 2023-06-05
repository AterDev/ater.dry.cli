using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Application.Implement;

public partial class DomainManagerBase<TEntity, TUpdate, TFilter, TItem> : ManagerBase<TEntity, TUpdate, TFilter, TItem>
    where TEntity : EntityBase
    where TFilter : FilterBase
{
    public DomainManagerBase(DataStoreContext storeContext) : base(storeContext)
    {
    }

    public DomainManagerBase(DataStoreContext storeContext, ILogger logger) : base(storeContext, logger)
    {
    }

    public DomainManagerBase(DataStoreContext storeContext, IUserContext userContext, ILogger logger) : base(storeContext, userContext, logger)
    {
    }
}