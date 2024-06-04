using MainApp.DTO.Music;
using MainApp.Models.Music;

namespace MainApp.Interfaces.Music
{
    public interface IMusicService
    {
        // Add new uploaded track
        Task AddTrackAsync(MusicTrackModelDTO musicTrackModel, string userId);

        // Get music styles
        Task<List<Style>> GetMusicStylesAsync();
    }
}
