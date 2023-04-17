namespace Core.Infrastructure;

public static class TplConst
{
    // tpl content varialbes
    public const string ID_TYPE = @"${IdType}";
    public const string COMMENT = @"${Comment}";
    public const string NAMESPACE = @"${Namespace}";
    public const string ENTITY_NAMESPACE = @"${EntityNamespace}";
    /// <summary>
    /// 控制器后缀
    /// </summary>
    public const string API_SUFFIX = @"${APISuffix}";
    public const string ENTITY_NAME = @"${EntityName}";
    public const string DEPEND_STORE = @"${DependStore}";
    public const string DBCONTEXT_NAME = @"${DbContextName}";
    public const string ADDITION_ACTION = @"${AdditionAction}";
    public const string SHARE_NAMESPACE = @"${ShareNamespace}";
    public const string STORE_NAMESPACE = @"${StoreNamespace}";
    public const string CREATEDTIME_NAME = @"${CreatedTimeName}";

    /// <summary>
    /// 注入服务
    /// </summary>
    public const string SERVICE_STORES = @"${StoreServices}";
    public const string SERVICE_MANAGER = @"${ManagerServices}";
    public const string DATASTORE_CONTEXT = @"${DataStoreContext}";
    /// <summary>
    /// 属性
    /// </summary>
    public const string STORECONTEXT_PROPS = @"${Properties}";
    /// <summary>
    /// 构造方法参数
    /// </summary>
    public const string STORECONTEXT_PARAMS = @"${CtorParams}";
    /// <summary>
    /// 构造方法赋值
    /// </summary>
    public const string STORECONTEXT_ASSIGN = @"${CtorAssign}";

    // protobuf
    public const string PROTOBUF_SERVICES = @"${Services}";
    public const string PROTOBUF_MESSAGES = @"${Messages}";
    public const string PROTOBUF_NAMESPACE = @"${Namespace}";

    // tpl names
}

public static class GenConst
{
    // generate file names
    public const string EXTIONSIONS_NAME = "Extensions.cs";
    public const string RESTAPI_BASE_NAME = "RestControllerBase.cs";
    public const string IRESTAPI_BASE_NAME = "IRestController.cs";
    public const string GLOBAL_USING_NAME = "GlobalUsings.cs";
}