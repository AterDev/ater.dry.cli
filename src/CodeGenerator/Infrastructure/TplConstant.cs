namespace CodeGenerator.Infrastructure;

public static class TplConstant
{
    // tpl content varialbes
    public const string COMMENT = @"${Comment}";
    public const string NAMESPACE = @"${Namespace}";
    public const string ENTITY_NAME = @"${EntityName}";
    public const string DBCONTEXT_NAME = @"${DbContextName}";
    public const string SHARE_NAMESPACE = @"${ShareNamespace}";
    public const string STORE_NAMESPACE = @"${StoreNamespace}";
    public const string DATASTORE_SERVICES = @"//${DataStoreServices}";


    // tpl names
    public const string IRESTAPI_BASE_NAME ="IRestApiBase.cs";
    public const string RESTAPI_BASE_NAME ="RestApiBase.cs";
}
