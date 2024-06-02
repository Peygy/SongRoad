using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainApp.Models.Music
{
    /// <summary>
    /// User's liked music tracks
    /// </summary>
    public class UserTracks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserId { get; set; } = null!;
        public List<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
    }
}
