using Entity;
namespace Share.Models.GenStepDtos;
/// <summary>
/// task step列表元素
/// </summary>
/// <see cref="Entity.GenStep"/>
public class GenStepItemDto
{
    /// <summary>
    /// step type
    /// </summary>
public GenStepType GenStepType { get; set; }
public Guid Id { get; set; } = Guid.NewGuid();
public DateTimeOffset CreatedTime { get; set; }
    
}
