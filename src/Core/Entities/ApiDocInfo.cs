namespace Core.Entities;
/// <summary>
/// 接口文档
/// </summary>
public class ApiDocInfo : EntityBase
{
    /// <summary>
    /// 文档名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 文档描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 文档地址
    /// </summary>
    public required string Path { get; set; }

}
