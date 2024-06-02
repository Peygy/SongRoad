using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music's album
    /// </summary>
    public class Album
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRequired]
        public string Title { get; set; } = null!;
        [BsonRequired]
        public int CreationYear { get; set; }
        // List of album music tracks
        public List<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
    }
}
