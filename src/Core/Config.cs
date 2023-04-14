namespace Core;

public static class Config
{
    public static string IdType { get; set; } = "Guid";
    public static string CreatedTimeName { get; set; } = "CreatedTime";
    public static string DtoPath { get; set; } = "Share";
    public static string EntityPath { get; set; } = "Core";
    public static string DbContextPath { get; set; } = "Database/EntityFramework";
    public static string StorePath { get; set; } = "Application";
    public static string ApiPath { get; set; } = "Http.API";
    /// <summary>
    /// 是否拆分
    /// </summary>
    public static bool? IsSplitController { get; set; } = false;

    public static readonly string ConfigFileName = ".droplet-config.json";
    public static readonly string StudioFileName = "AterStudio.dll";
    /// <summary>
    /// 存储ts枚举类
    /// </summary>
    public static List<string> EnumModels = new();


    public static void SetConfig(ConfigOptions configOptions)
    {
        Config.IdType = configOptions.IdType;
        Config.IsSplitController = configOptions.IsSplitController;
        Config.EntityPath = configOptions.EntityPath;
        Config.StorePath = configOptions.StorePath;
        Config.ApiPath = configOptions.ApiPath;
        Config.DtoPath = configOptions.DtoPath;
        Config.CreatedTimeName = configOptions.CreatedTimeName;
    }
}
