namespace Share.Entity;

/// <summary>
/// 操作与步骤中间表
/// </summary>
public class GenActionGenStep : EntityBase
{
    /// <summary>
    /// GenAction id
    /// </summary>
    public int GenActionId { get; set; }

    /// <summary>
    /// GenStep id
    /// </summary>
    public int GenStepId { get; set; }
}
