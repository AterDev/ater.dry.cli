using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;
using Core.Infrastructure.Helper;
using LiteDB;

namespace Core.Models;
/// <summary>
/// 项目配置
/// </summary>
public class ConfigOptions
{
    /// <summary>
    /// 项目根目录 
    /// </summary>
    public string RootPath { get; set; } = "./";

    public Guid ProjectId { get; set; }
    /// <summary>
    /// dto项目目录
    /// </summary>
    public string DtoPath { get; set; } = "src/Share";
    public string EntityPath { get; set; } = "src/Core";
    public string DbContextPath { get; set; } = "src/Database/EntityFramework";
    public string StorePath { get; set; } = "src/Application";
    public string ApiPath { get; set; } = "src/Http.API";

    /// <summary>
    /// NameId/Id
    /// </summary>
    public string IdStyle { get; set; } = "Id";
    public string IdType { get; set; } = "Guid";
    public string CreatedTimeName { get; set; } = "CreatedTime";
    public string UpdatedTimeName { get; set; } = "UpdatedTime";

    /// <summary>
    /// 是否拆分
    /// </summary>
    public bool? IsSplitController { get; set; } = false;

    [JsonConverter(typeof(DoubleStringJsonConverter))]
    public string Version { get; set; } = "7.1";
    /// <summary>
    /// swagger地址
    /// </summary>
    public string? SwaggerPath { get; set; }
    /// <summary>
    /// 前端路径
    /// </summary>
    public string? WebAppPath { get; set; }
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