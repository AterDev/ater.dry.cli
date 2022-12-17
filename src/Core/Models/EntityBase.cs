using System.ComponentModel.DataAnnotations;
using LiteDB;

namespace Core.Models;

public class EntityBase
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    [MaxLength(60)]
    public required string ProjectId { get; set; }
}
