namespace ${Namespace}.Models;
public partial class EntityBase
{
    public ${IdType} Id { get; set; } = default!;
    public DateTimeOffset ${CreatedTimeName} { get; set; }  = default!;
}
