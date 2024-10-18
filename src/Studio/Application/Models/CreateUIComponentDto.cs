using System.ComponentModel;
using Share.Models;

namespace Application.Models;
/// <summary>
/// 生成组件模型
/// </summary>
public class CreateUIComponentDto
{
    public UIType UIType { get; set; } = UIType.AngularMaterial;
    /// <summary>
    /// 组件类型
    /// </summary>
    public ComponentType ComponentType { get; set; }
    /// <summary>
    /// model info
    /// </summary>
    public required ModelInfo ModelInfo { get; set; }
    public required string ServiceName { get; set; }
}

/// <summary>
/// 使用的UI组件库
/// </summary>
public enum UIType
{
    AngularMaterial,
    NGZORRO,
    VueElement
}
public enum ComponentType
{
    /// <summary>
    /// 提交表单
    /// </summary>
    [Description("提交表单")]
    Form,
    /// <summary>
    /// 展示表格
    /// </summary>
    [Description("展示表格")]
    Table,
    /// <summary>
    /// 详情字段
    /// </summary>
    [Description("详情字段")]
    Detail
}
