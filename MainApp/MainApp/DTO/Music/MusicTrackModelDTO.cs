using MainApp.Models.Music;

namespace MainApp.DTO.Music
{
    public class MusicTrackModelDTO
    {
        public string Title { get; set; } = null!;
        public string Style { get; set; } = null!;
        public string MusicId { get; set; } = null!;
        public string ImageBase64 { get; set; } = null!;

        public bool isLiked { get; set; }
        public string CreatorName { get; set; } = null!;

        public MusicTrackModelDTO(MusicTrack musicTrack)
        {
            Title = musicTrack.Title;
            Style = musicTrack.Style.Name;
            MusicId = musicTrack.Id;
            ImageBase64 = Convert.ToBase64String(musicTrack.TrackImage.ImageData);
        }

        public MusicTrackModelDTO(MusicTrack musicTrack, MusicAuthor authorModel) : this(musicTrack)
        {
            isLiked = authorModel.LikedTracksId.Any(m => m == musicTrack.Id);
            CreatorName = authorModel.Name;
        }
    }
}
