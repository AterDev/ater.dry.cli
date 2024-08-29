namespace GeneratorForNode;

public static class TplConst
{
    // tpl content varialbes
    public const string ID_TYPE = "#@IdType#";
    public const string COMMENT = "#@Comment#";
    public const string NAMESPACE = "#@Namespace#";
    public const string ENTITY_NAMESPACE = "#@EntityNamespace#";
    /// <summary>
    /// 控制器后缀
    /// </summary>
    public const string API_SUFFIX = "#@APISuffix#";
    public const string ENTITY_NAME = "#@EntityName#";
    public const string ENTITY_PATH_NAME = "#@EntityPathName#";
    public const string SERVICE_NAME = "#@ServiceName#";
    public const string MODULE_NAME = "#@ModuleName#";
    public const string MODULE_PATH_NAME = "#@ModulePathName#";
    public const string ROUTE_PATH_NAME = "#@RoutePathName#";

    public const string MODEL_NAME = "#@ModelName#";
    public const string SERVICE_PATH_NAME = "#@ServicePathName#";
    public const string MODEL_PATH_NAME = "#@ModelPathName#";
    public const string DEFINED_FORM_CONTROLS = "//[@DefinedFormControls]";
    public const string DEFINED_PROPERTIES = "//[@DefinedProperties]";
    public const string DEFINED_VALIDATOR_MESSAGE = "//[@DefinedValidatorMessage]";
    public const string COLUMNS_DEF = "//[@ColumnsDef]";
    public const string COLUMNS = "#@Columns#";
    public const string IMPORT_MODULES = "#@#ImportModules";
    public const string IMPORT_MODULES_PATH = "#@ImportMODULESPATH#";
    public const string CONTENT = "//[@Content]";
    public const string IMPORTS = "//[@Imports]";
    public const string DECLARES = "//[@Declares]";
    public const string ENUM_BLOCKS = "//[@EnumBlocks]";
    public const string DI = "//[@DI]";
    public const string METHODS = "//[@Methods]";
    public const string INIT = "//[@Init]";
    public const string FORM_CONTROLS = "//[@FormControls]";
    public const string FILTER_FORM = "//[@FilterForm]";

    // manager tpl
    public const string ADD_ACTION_BLOCK = "//[@AddActionBlock]";
    public const string UPDATE_ACTION_BLOCK = "//[@UpdateActionBlock]";
    public const string FILTER_ACTION_BLOCK = "//[@FilterActionBlock]";

    // controller tpl
    public const string ADDICTION_MANAGER_DI = "//[@AdditionManagersDI]";
    public const string ADDICTION_MANAGER_PROPS = "//[@AdditionManagersProps]";


    public const string DEPEND_STORE = "#@DependStore#";
    public const string DBCONTEXT_NAME = "#@DbContextName#";
    public const string ADDITION_ACTION = "#@AdditionAction#";
    public const string SHARE_NAMESPACE = "#@ShareNamespace#";
    public const string STORE_NAMESPACE = "#@StoreNamespace#";
    public const string CREATEDTIME_NAME = "#@CreatedTimeName#";


    /// <summary>
    /// 注入服务
    /// </summary>
    public const string SERVICE_STORES = "#@StoreServices#";
    public const string SERVICE_MANAGER = "#@ManagerServices#";
    public const string DATASTORE_CONTEXT = "#@DataStoreContext#";
    /// <summary>
    /// 属性
    /// </summary>
    public const string STORECONTEXT_PROPS = "#@Properties#";
    /// <summary>
    /// 构造方法参数
    /// </summary>
    public const string STORECONTEXT_PARAMS = "#@CtorParams#";
    /// <summary>
    /// 构造方法赋值
    /// </summary>
    public const string STORECONTEXT_ASSIGN = "#@CtorAssign#";

    // protobuf
    public const string PROTOBUF_SERVICES = "#@Services#";
    public const string PROTOBUF_MESSAGES = "#@Messages#";
    public const string PROTOBUF_NAMESPACE = "#@Namespace#";

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