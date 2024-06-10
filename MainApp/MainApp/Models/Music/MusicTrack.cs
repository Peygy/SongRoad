using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track
    /// </summary>
    public class MusicTrack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonRequired]
        public string Title { get; set; } = null!;
        // Music style, e.x rock, jazz
        [BsonRequired]
        public Style Style { get; set; } = null!;
        public TrackImageModel TrackImage { get; set; } = null!;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public string CreatorId { get; set; } = null!;
    }
}