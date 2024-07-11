using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track style
    /// </summary>
    public class Style
    {
        [Key]
        public ObjectId Id { get; set; }
        public string Name { get; set; } = null!;

        public List<MusicTrack> MusicTracks { get; set; } = new List<MusicTrack>();
    }
}
