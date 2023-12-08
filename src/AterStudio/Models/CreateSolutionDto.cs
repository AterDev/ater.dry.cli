using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AterStudio.Models;

/// <summary>
/// 创建解决方案dto
/// </summary>
public class CreateSolutionDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(300)]
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
    [MaxLength(300)]
    public string? CommandDbConnStrings { get; set; }
    /// <summary>
    /// 读数据库连接字符串
    /// </summary>
    [MaxLength(300)]
    public string? QueryDbConnStrings { get; set; }
    /// <summary>
    /// 缓存连接字符串
    /// </summary>
    [MaxLength(200)]
    public string? CacheConnStrings { get; set; }
    /// <summary>
    /// 缓存实例名称
    /// </summary>
    [MaxLength(60)]
    public string? CacheInstanceName { get; set; }
    /// <summary>
    /// 内容管理模块
    /// </summary>
    public bool HasCmsFeature { get; set; } = false;
    /// <summary>
    /// 用户文件模块
    /// </summary>
    public bool HasFileManagerFeature { get; set; } = false;

    /// <summary>
    /// 订单模块
    /// </summary>
    public bool HasOrderFeature { get; set; } = false;

    /// <summary>
    /// 项目类型
    /// </summary>
    public ProjectType ProjectType { get; set; } = ProjectType.WebAPI;
}

public enum DBType
{
    /// <summary>
    /// PostgreSQL
    /// </summary>
    [Description("PostgreSQL")]
    PostgreSQL,
    /*/// <summary>
    /// SQLServer
    /// </summary>
    [Description("SQLServer")]
    SQLServer*/

}
public enum CacheType
{
    /// <summary>
    /// Redis
    /// </summary>
    [Description(description: "Redis")]
    Redis,
    /// <summary>
    /// Memory
    /// </summary>
    [Description(description: "Memory")]
    Memory
}