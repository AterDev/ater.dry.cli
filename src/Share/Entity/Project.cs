namespace Entity;
/// <summary>
/// 项目
/// </summary>
public class Project : EntityBase
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    /// <summary>
    /// 显示名
    /// </summary>
    [MaxLength(100)]
    public required string DisplayName { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(200)]
    public required string Path { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [MaxLength(20)]
    public string? Version { get; set; }
    /// <summary>
    /// 解决方案类型
    /// </summary>
    public SolutionType? SolutionType { get; set; }

    /// <summary>
    /// Front Path
    /// </summary>
    [MaxLength(200)]
    public string? FrontPath { get; set; }

    /// <summary>
    /// project config
    /// </summary>
    public ProjectConfig? Config { get; set; }

    public List<EntityInfo> EntityInfos { get; set; } = [];
    public List<ApiDocInfo> ApiDocInfos { get; set; } = [];

    public ICollection<GenAction> GenActions { get; set; } = [];

    public ICollection<GenStep> GenSteps { get; set; } = [];
}

/// <summary>
///  项目配置
/// </summary>
public class ProjectConfig
{
    public string IdType { get; private set; } = "Guid";
    public string CreatedTimeName { get; private set; } = "CreatedTime";
    public string UpdatedTimeName { get; private set; } = "UpdatedTime";
    public string Version { get; private set; } = "8.0";
    public string SharePath { get; private set; } = "src/Definition/Share";
    public string EntityPath { get; private set; } = "src/Definition/Entity";
    public string EntityFrameworkPath { get; private set; } = "src/Definition/EntityFramework";
    public string ApplicationPath { get; private set; } = "src/Application";
    public string ApiPath { get; private set; } = "src/Http.API";
    public string MicroservicePath { get; private set; } = "src/Microservice";
    public string SolutionPath { get; set; } = "";
    public bool IsLight { get; private set; } = false;
    public ControllerType ControllerType { get; set; } = ControllerType.Client;

    /// <summary>
    /// 是否拆分
    /// </summary>
    public bool? IsSplitController { get; set; } = false;
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
public enum SolutionType
{
    DotNet,
    Node
}
