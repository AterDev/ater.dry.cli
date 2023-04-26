using Core.Entities;

namespace AterStudio.Models;

/// <summary>
/// 接口返回模型
/// </summary>
public class ApiDocContent
{
    /// <summary>
    /// 接口信息
    /// </summary>
    public List<RestApiGroup> RestApiGroups { get; set; } = new List<RestApiGroup>();
    /// <summary>
    /// 所有请求及返回类型信息
    /// </summary>
    public List<EntityInfo> ModelInfos { get; set; } = new List<EntityInfo>();
    /// <summary>
    /// tag信息
    /// </summary>
    public List<ApiDocTag> OpenApiTags { get; set; } = new List<ApiDocTag>();
}
