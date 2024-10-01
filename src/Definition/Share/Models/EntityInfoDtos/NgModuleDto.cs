namespace Share.Models.EntityInfoDtos;
public class NgModuleDto
{
    /// <summary>
    /// 实体名
    /// </summary>
    public required string EntityName { get; set; }
    /// <summary>
    /// 目标路径
    /// </summary>
    public required string RootPath { get; set; }
    /// <summary>
    /// 是否移动端
    /// </summary>
    public bool IsMobile { get; set; }
}
