using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track author
    /// </summary>
    public class MusicAuthor
    {
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;

        public List<ObjectId> LikedTracks { get; set; } = new List<ObjectId>();
    }
}
