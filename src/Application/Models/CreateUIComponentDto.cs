using Definition.Entity;

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
    public required EntityInfo ModelInfo { get; set; }
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
    Form,
    Table
}
