using MainApp.DTO.Music;
using MainApp.Models.Music;

namespace MainApp.Interfaces.Music
{
    public interface IMusicService
    {
        // Add new uploaded track
        Task AddTrackAsync(NewMusicTrackModelDTO musicTrackModel, string userId);

        // Return list of user uploaded tracks
        Task<List<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId);

        // Return file stream
        Task<Stream> GetMusicTrackStreamAsync(string trackId);

        // Get music styles
        Task<List<Style>> GetMusicStylesAsync();
    }
}
