namespace Share.Models.GenStepDtos;
/// <summary>
/// task step筛选条件
/// </summary>
/// <see cref="Entity.GenStep"/>
public class GenStepFilterDto : FilterBase
{
    /// <summary>
    /// step type
    /// </summary>
    public GenStepType? GenStepType { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? GenActionId { get; set; }

}
