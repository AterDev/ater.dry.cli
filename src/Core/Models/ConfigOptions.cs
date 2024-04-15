using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

using Core.Entities;

namespace Core.Models;
/// <summary>
/// 项目配置
/// </summary>
public class ConfigOptions
{
    /// <summary>
    /// 项目根目录 
    /// </summary>
    public string RootPath { get; set; } = Path.Combine("./");
    /// <summary>
    /// 是否为轻量项目
    /// </summary>
    public bool IsLight { get; set; }
    public Guid ProjectId { get; set; }
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string DtoPath { get; set; } = Config.SharePath;
    public string EntityPath { get; set; } = Config.EntityPath;
    public string DbContextPath { get; set; } = Config.EntityFrameworkPath;
    public string ApplicationPath { get; set; } = Config.ApplicationPath;
    public string ApiPath { get; set; } = Config.ApiPath;
    /// <summary>
    /// NameId/Id
    /// </summary>
    public string IdStyle { get; set; } = "Id";
    public string IdType { get; set; } = Config.IdType;
    public string CreatedTimeName { get; set; } = Config.CreatedTimeName;
    public string UpdatedTimeName { get; set; } = Config.UpdatedTimeName;

    /// <summary>
    /// 控制器是否拆分
    /// </summary>
    public bool? IsSplitController { get; set; } = false;
    public ControllerType ControllerType { get; set; } = Config.ControllerType;

    [JsonConverter(typeof(DoubleStringJsonConverter))]
    public string Version { get; set; } = Config.Version;

    /// <summary>
    /// 前端路径
    /// </summary>
    public string? WebAppPath { get; set; }
    public SolutionType? SolutionType { get; set; }

    public static ConfigOptions? ParseJson(string jsonString)
    {
        return JsonSerializer.Deserialize<ConfigOptions>(jsonString, new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });
    }
}
public enum ControllerType
{
    /// <summary>
    /// 用户端
    /// </summary>
    [Description("用户端")]
    Client,
    /// <summary>
    /// 管理端
    /// </summary>
    [Description("管理端")]
    Admin,
    /// <summary>
    /// 用户和管理端
    /// </summary>
    [Description("用户端和管理端")]
    Both

}

public class DoubleStringJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Number ? reader.GetDouble().ToString() : reader.GetString() ?? "";
    }


    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
}