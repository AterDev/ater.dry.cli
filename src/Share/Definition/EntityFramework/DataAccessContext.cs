using Ater.Web.Abstraction.EntityFramework;
using Share.EntityFramework.DBProvider;

namespace Share.EntityFramework;
/// <summary>
/// 数据访问层抽象
/// </summary>
public class DataAccessContext<TEntity>(CommandDbContext commandDbContext, QueryDbContext queryDbContext) : DataAccessContextBase<CommandDbContext, QueryDbContext, TEntity>(commandDbContext, queryDbContext)
    where TEntity : class, IEntityBase
{
}
