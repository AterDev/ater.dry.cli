using Microsoft.OpenApi.Models;

namespace AterStudio.Models;

/// <summary>
/// 接口返回模型
/// </summary>
public class ApiDocContent
{
    /// <summary>
    /// 接口信息
    /// </summary>
    public List<RestApiInfo> RestApiInfos { get; set; } = new List<RestApiInfo>();
    /// <summary>
    /// 所有请求及返回类型信息
    /// </summary>
    public List<EntityInfo> ModelInfos { get; set; } = new List<EntityInfo>();
    /// <summary>
    /// tag信息
    /// </summary>
    public List<OpenApiTag> OpenApiTags { get; set; } = new List<OpenApiTag>();
}
