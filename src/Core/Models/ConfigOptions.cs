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
    public string DtoPath { get; set; } = Path.Combine("src", "Share");
    public string EntityPath { get; set; } = Path.Combine("src", "Entity");
    public string DbContextPath { get; set; } = Path.Combine("src", "Database", "EntityFramework");
    /// <summary>
    /// 废弃属性
    /// </summary>
    public string? StorePath { get; set; }
    public string ApplicationPath { get; set; } = Path.Combine("src", "Application");
    public string ApiPath { get; set; } = Path.Combine("src", "Http.API");
    /// <summary>
    /// NameId/Id
    /// </summary>
    public string IdStyle { get; set; } = "Id";
    public string IdType { get; set; } = "Guid";
    public string CreatedTimeName { get; set; } = "CreatedTime";
    public string UpdatedTimeName { get; set; } = "UpdatedTime";

    /// <summary>
    /// 控制器是否拆分
    /// </summary>
    public bool? IsSplitController { get; set; } = false;

    [JsonConverter(typeof(DoubleStringJsonConverter))]
    public string Version { get; set; } = "7.0.0";

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
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetDouble().ToString();
        }
        else
        {
            return reader.GetString() ?? "";
        }
    }


    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
}