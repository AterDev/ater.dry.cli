namespace CodeGenerator;

public static class Config
{
    public static readonly string ConfigPath = "./.gtcli-config.json";

    public static string ENTITY_NAMESPACE = "Entity";
    public static string SERVICE_NAMESPACE = "Services";
    public static string WEB_NAMESPACE = "App.Api";
    public static string SHARE_NAMESPACE = "Share";
    public static string DBCONTEXT_NAMESPACE = "EntityFrameworkCore";

    public static string CLIENT_PATH = "../clients/webapp";
    public static string API_PATH = "./App.Api";
    public static string SERVICE_PATH = "./Services";
    public static string SHARE_PATH = "./Share";
    public static string DBCONTEXT_PATH = "./EntityFrameworkCore";
    public static string DTO_PATH = "./Share/Models";
}
