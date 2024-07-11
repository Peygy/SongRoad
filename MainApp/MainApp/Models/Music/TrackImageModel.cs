using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MainApp.Models.Music
{
    public class TrackImageModel
    {
        [Key]
        public ObjectId Id { get; set; }
        public string ContentType { get; set; } = null!;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
    }
}
