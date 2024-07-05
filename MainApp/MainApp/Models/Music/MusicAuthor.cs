using MongoDB.Bson.Serialization.Attributes;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track author
    /// </summary>
    public class MusicAuthor
    {
        [BsonId]
        public string? Id { get; set; }
        [BsonRequired]
        public string Name { get; set; } = null!;

        public List<string> UploadedTracksId { get; set; } = new List<string>();
        public List<string> LikedTracksId { get; set; } = new List<string>();
    }
}
