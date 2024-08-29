using Ater.Web.Abstraction.EntityFramework;

using EntityFramework.DBProvider;

namespace Definition.EntityFramework;
/// <summary>
/// 数据访问层抽象
/// </summary>
public class DataAccessContext<TEntity>(CommandDbContext commandDbContext, QueryDbContext queryDbContext) : DataAccessContextBase<CommandDbContext, QueryDbContext, TEntity>(commandDbContext, queryDbContext)
    where TEntity : class, IEntityBase
{
}
