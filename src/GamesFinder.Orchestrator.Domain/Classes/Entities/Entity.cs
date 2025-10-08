using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamesFinder.Orchestrator.Domain.Classes.Entities;

public abstract class Entity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
}