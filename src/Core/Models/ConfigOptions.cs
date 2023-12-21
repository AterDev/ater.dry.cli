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
    public Guid ProjectId { get; set; }
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string DtoPath { get; set; } = Config.SharePath;
    public string EntityPath { get; set; } = Config.EntityPath;
    public string DbContextPath { get; set; } = Config.EntityFrameworkPath;
    /// <summary>
    /// 废弃属性
    /// </summary>
    public string? StorePath { get; set; }
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

public class DoubleStringJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Number ? reader.GetDouble().ToString() : reader.GetString() ?? "";
    }


    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
}