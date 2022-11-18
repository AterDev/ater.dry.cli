namespace Core;

public static class Config
{
    public static string IdType = "Guid";
    public static string CreatedTimeName = "CreatedTime";
    public static readonly string ConfigFileName = ".droplet-config.json";
    public static readonly string StudioFileName = "Studio.dll";


    /// <summary>
    /// 存储ts枚举类
    /// </summary>
    public static List<string> EnumModels = new();
}
