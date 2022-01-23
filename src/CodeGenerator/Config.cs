namespace CodeGenerator;

public static class Config
{
    public static readonly string ConfigPath = "./.gtcli-config.json";

    // namespace
    public static string SHARE_NAMESPACE         = "Share";
    public static string ENTITY_NAMESPACE        = "Entity";
    public static string HTTPAPI_NAMESPACE       = "App.Api";
    public static string SERVICE_NAMESPACE       = "Services";
    public static string DBCONTEXT_NAMESPACE     = "EntityFrameworkCore";

    // path
    public static string SHARE_PATH                 = "./Share";
    public static string CLIENT_PATH                = "../clients";
    public static string HTTPAPI_PATH               = "./Http.API";
    public static string SERVICE_PATH               = "./Services";
    public static string SHAREMODEL_PATH            = "./Share/Models";
    public static string DBCONTEXT_PATH             = "./EntityFrameworkCore";


}
