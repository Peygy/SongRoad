using MongoDB.Bson;

namespace MainApp.DTO.Music
{
    public class NewMusicTrackModelDTO
    {
        public string Title { get; set; } = null!;
        public ObjectId StyleId { get; set; }
        public IFormFile Mp3File { get; set; } = null!;
        public IFormFile TrackImage { get; set; } = null!;
    }
}
