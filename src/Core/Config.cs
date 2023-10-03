namespace Core;

public static class Config
{
    public static string IdType { get; set; } = "Guid";
    public static string CreatedTimeName { get; set; } = "CreatedTime";
    public static string Version { get; private set; } = "8.0";
    public static string SharePath { get; set; } = Path.Combine("src", "Share");
    public static string EntityPath { get; set; } = Path.Combine("src", "Entity");
    public static string EntityFrameworkPath { get; set; } = Path.Combine("src", "Database", "EntityFramework");
    public static string ApplicationPath { get; set; } = Path.Combine("src", "Application");
    public static string ApiPath { get; set; } = Path.Combine("src", "Http.API");
    public static string WebAppPath { get; set; } = "";

    public static string SolutionPath { get; set; } = "";

    /// <summary>
    /// 是否拆分
    /// </summary>
    public static bool? IsSplitController { get; set; } = false;

    public static readonly string ConfigFileName = ".dry-config.json";
    public static readonly string StudioFileName = "AterStudio.dll";
    /// <summary>
    /// 存储ts枚举类
    /// </summary>
    public static List<string> EnumModels = new();


    public static void SetConfig(ConfigOptions configOptions)
    {
        IdType = configOptions.IdType;
        IsSplitController = configOptions.IsSplitController;
        EntityPath = configOptions.EntityPath;
        ApplicationPath = configOptions.ApplicationPath;
        ApiPath = configOptions.ApiPath;
        SharePath = configOptions.DtoPath;
        CreatedTimeName = configOptions.CreatedTimeName;
        Version = configOptions.Version;
        WebAppPath = configOptions.WebAppPath ?? "";
    }
}
