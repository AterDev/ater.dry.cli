namespace Share.Models;
/// <summary>
/// csproj 程序集信息
/// </summary>
public class AssemblyInfo
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public required string FileName { get; set; }
    /// <summary>
    /// 程序集名称
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// 命名空间
    /// </summary>
    public string? Namespace { get; set; }
    /// <summary>
    /// 项目类型
    /// </summary>
    public AssemblyType Type { get; set; }
    /// <summary>
    /// grpc类型
    /// </summary>
    public GrpcType? GrpcType { get; set; } = Share.Models.GrpcType.Default;

}

public enum AssemblyType
{
    Console,
    WebApi,
    Grpc,
    Test,
    Lib,
    Maui
}

public enum GrpcType
{
    Default,
    Client,
    Server,
    Both
}