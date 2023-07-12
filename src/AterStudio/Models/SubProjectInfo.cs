namespace AterStudio.Models;
/// <summary>
/// 项目信息
/// </summary>
public class SubProjectInfo
{
    public required string Name { get; set; }
    public required string Path { get; set; }

    public ProjectType ProjectType { get; set; } = ProjectType.Web;
}

public enum ProjectType
{
    /// <summary>
    /// web服务
    /// </summary>
    Web,
    /// <summary>
    /// 控制台应用
    /// </summary>
    Console,
    /// <summary>
    /// 类库
    /// </summary>
    Lib,
    /// <summary>
    /// 模块
    /// </summary>
    Module,
    /// <summary>
    /// 接口服务
    /// </summary>
    WebAPI,
    /// <summary>
    /// gPRC服务
    /// </summary>
    GRPC
}
