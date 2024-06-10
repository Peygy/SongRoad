using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MainApp.Models.Music
{
    public class TrackImageModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
    }
}
