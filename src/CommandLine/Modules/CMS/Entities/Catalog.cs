using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities.CmsEntities;
/// <summary>
/// 目录
/// </summary>
[Index(nameof(Name))]
[Index(nameof(Level))]
[Module("CMS")]
public class Catalog : EntityBase, ITreeNode<Catalog>
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }

    /// <summary>
    /// 层级
    /// </summary>
    public short Level { get; set; } = 0;
    /// <summary>
    /// 子目录
    /// </summary>
    public List<Catalog>? Children { get; set; }

    /// <summary>
    /// 父目录
    /// </summary>
    [ForeignKey(nameof(ParentId))]
    public Catalog? Parent { get; set; }
    public Guid? ParentId { get; set; }
    public List<Blog>? Blogs { get; set; }
    public required User User { get; set; }
}