namespace Core;

public static class Config
{
    public static string IdType { get; private set; } = "Guid";
    public static string CreatedTimeName { get; private set; } = "CreatedTime";
    public static string UpdatedTimeName { get; private set; } = "UpdatedTime";
    public static string Version { get; private set; } = "8.0";
    public static string SharePath { get; private set; } = Path.Combine("src", "Definition", "Share");
    public static string EntityPath { get; private set; } = Path.Combine("src", "Definition", "Entity");
    public static string EntityFrameworkPath { get; private set; } = Path.Combine("src", "Definition", "EntityFramework");
    public static string ApplicationPath { get; private set; } = Path.Combine("src", "Application");
    public static string ApiPath { get; private set; } = Path.Combine("src", "Http.API");
    public static string MicroservicePath { get; private set; } = Path.Combine("src", "Microservice");
    public static string SolutionPath { get; set; } = "";
    public static bool IsLight { get; private set; } = false;
    public static ControllerType ControllerType { get; set; } = ControllerType.Client;

    /// <summary>
    /// 是否拆分
    /// </summary>
    public static bool? IsSplitController { get; set; } = false;

    /// <summary>
    /// 是否为微服务
    /// </summary>
    public static bool IsMicroservice { get; set; }

    /// <summary>
    /// 微服务名称
    /// </summary>
    public static string? ServiceName { get; set; } = null;

    public static readonly string ConfigFileName = ".dry-config.json";
    public static readonly string StudioFileName = "AterStudio.dll";
    /// <summary>
    /// 存储ts枚举类
    /// </summary>
    public static List<string> EnumModels { get; set; } = [];


    public static void SetConfig(ConfigOptions configOptions)
    {
        IdType = configOptions.IdType;
        IsSplitController = configOptions.IsSplitController;
        EntityPath = configOptions.EntityPath;
        ApplicationPath = configOptions.ApplicationPath;
        ApiPath = configOptions.ApiPath;
        SharePath = configOptions.DtoPath;
        EntityFrameworkPath = configOptions.DbContextPath;
        CreatedTimeName = configOptions.CreatedTimeName;
        Version = configOptions.Version;
        IsMicroservice = false;
        IsLight = configOptions.IsLight;
        ControllerType = configOptions.ControllerType;
        ServiceName = null;
    }

    /// <summary>
    /// 获取服务的默认路径
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public static ConfigOptions GetServiceConfig(string serviceName)
    {
        return new ConfigOptions
        {
            RootPath = Path.Combine(MicroservicePath, serviceName),
            ApiPath = Path.Combine(MicroservicePath, serviceName),
            ApplicationPath = Path.Combine(MicroservicePath, serviceName, "Application"),
            DbContextPath = Path.Combine(MicroservicePath, serviceName, "Definition", "EntityFramework"),
            DtoPath = Path.Combine(MicroservicePath, serviceName, "Definition", "Share"),
            EntityPath = Path.Combine(MicroservicePath, serviceName, "Definition", "Entity"),
        };
    }
}
