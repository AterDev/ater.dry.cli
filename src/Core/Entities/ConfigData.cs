using System.ComponentModel.DataAnnotations;
using LiteDB;

namespace Core.Entities;
/// <summary>
/// 配置
/// </summary>
public class ConfigData
{

    public const string OpenAI = "openAIKey";

    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// 键
    /// </summary>
    [MaxLength(100)]
    public required string Key { get; set; }
    /// <summary>
    /// 值 
    /// </summary>
    [MaxLength(2000)]
    public required string Value { get; set; }

    public ValueType ValueType { get; set; } = ValueType.String;
}

public enum ValueType
{
    Integer,
    Double,
    String,
    Boolean,
}
