using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track author
    /// </summary>
    public class MusicAuthor
    {
        [Key]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;

        public List<MusicTrack> UploadedTracks { get; set; } = new List<MusicTrack>();
        public List<MusicTrack> LikedTracks { get; set; } = new List<MusicTrack>();
    }
}
