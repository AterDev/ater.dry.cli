using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Infrastructure;

public static class TplConstant
{
    public const string NAMESPACE = @"${Namespace}";
    public const string ENTITY_NAME = @"${EntityName}";
    public const string DBCONTEXT_NAME = @"${DbContextName}";
    public const string SHARE_NAMESPACE = @"${ShareNamespace}";
    public const string DATASTORE_SERVICES = @"//${DataStoreServices}";
}
