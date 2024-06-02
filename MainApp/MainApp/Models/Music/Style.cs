using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track style
    /// </summary>
    public class Style
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRequired]
        public string Name { get; set; } = null!;
    }
}
