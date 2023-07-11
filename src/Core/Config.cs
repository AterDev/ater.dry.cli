﻿namespace Core;

public static class Config
{
    public static string IdType { get; set; } = "Guid";
    public static string CreatedTimeName { get; set; } = "CreatedTime";
    public static string Version { get; private set; } = "8.0";
    public static string DtoPath { get; set; } = "src/Share";
    public static string EntityPath { get; set; } = "src/Entity";
    public static string DbContextPath { get; set; } = "src/Database/EntityFramework";
    public static string ApplicationPath { get; set; } = "src/Application";
    public static string ApiPath { get; set; } = "src/Http.API";

    public static string SwaggerPath { get; set; } = "";
    public static string WebAppPath { get; set; } = "";

    public static string SolutionPath { get; set; } = "";

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
        IdType = configOptions.IdType;
        IsSplitController = configOptions.IsSplitController;
        EntityPath = configOptions.EntityPath;
        ApplicationPath = configOptions.StorePath;
        ApiPath = configOptions.ApiPath;
        DtoPath = configOptions.DtoPath;
        CreatedTimeName = configOptions.CreatedTimeName;
        Version = configOptions.Version;
        WebAppPath = configOptions.WebAppPath ?? "";
        SwaggerPath = configOptions.SwaggerPath ?? "";
    }
}
