namespace AterStudio.Models;

/// <summary>
/// 创建解决方案dto
/// </summary>
public class CreateSolutionDto
{
    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DBType DBType { get; set; } = DBType.PostgreSQL;

    /// <summary>
    /// 缓存类型
    /// </summary>
    public CacheType CacheType { get; set; } = CacheType.Redis;

    /// <summary>
    /// 是否包含租户
    /// </summary>
    public bool HasTenant { get; set; }

    /// <summary>
    /// 是否包含验证授权服务
    /// </summary>
    public bool HasIdentityServer { get; set; } = false;
    /// <summary>
    /// 是否包含任务管理服务
    /// </summary>
    public bool HasTaskManager { get; set; } = false;
    /// <summary>
    /// 写数据库连接字符串
    /// </summary>
    public string? CommandDbConnStrings { get; set; }
    /// <summary>
    /// 读数据库连接字符串
    /// </summary>
    public string? QueryDbConnStrings { get; set; }
    /// <summary>
    /// 缓存连接字符串
    /// </summary>
    public string? CacheConnStrings { get; set; }
    /// <summary>
    /// 缓存实例名称
    /// </summary>
    public string? CacheInstanceName { get; set; }

    /// <summary>
    /// 功能模块
    /// </summary>
    public string[]? Features { get; set; }
}

public enum DBType
{
    SQLServer,
    PostgreSQL
}
public enum CacheType
{
    Redis,
    Memory,
    None
}
