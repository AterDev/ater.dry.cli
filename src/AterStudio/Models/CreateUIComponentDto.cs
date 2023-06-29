using Core.Entities;

namespace AterStudio.Models;
/// <summary>
/// 生成组件模型
/// </summary>
public class CreateUIComponentDto
{
    public UIType UIType { get; set; } = UIType.AngularMaterial;

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
