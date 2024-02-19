using LiteDB;

namespace Core.Models;

public class EntityBase
{
    [BsonId]
    public Guid Id { get; set; } = Guid.NewGuid();

    public required Guid ProjectId { get; set; }
}
