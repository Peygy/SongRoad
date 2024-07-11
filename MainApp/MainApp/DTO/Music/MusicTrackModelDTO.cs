using MainApp.Models.Music;
using MongoDB.Bson;

namespace MainApp.DTO.Music
{
    public class MusicTrackModelDTO
    {
        public string Title { get; private set; } = null!;
        public string Style { get; private set; } = null!;
        public string MusicId { get; private set; } = null!;
        public string ImageBase64 { get; private set; } = null!;

        public bool isLiked { get; set; }
        public string CreatorName { get; set; } = null!;

        public MusicTrackModelDTO(MusicTrack musicTrack)
        {
            Title = musicTrack.Title;
            Style = musicTrack.Style.Name;
            MusicId = musicTrack.Id.ToString();
            if (musicTrack.TrackImage != null)
            {
                ImageBase64 = Convert.ToBase64String(musicTrack.TrackImage.ImageData);
            }

            if (musicTrack.Creator != null)
            {
                CreatorName = musicTrack.Creator.Name;
                isLiked = musicTrack.Creator.LikedTracks.Any(m => m == musicTrack.Id);
            }
        }
    }
}
