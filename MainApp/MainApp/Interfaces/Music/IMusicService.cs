using MainApp.DTO.Music;
using MainApp.Models.Music;
using MainApp.Models.User;

namespace MainApp.Interfaces.Music
{
    public interface IMusicService
    {
        // Check if author exist in database
        Task CheckAuthorExistAsync(UserModel? user);

        // Add new uploaded track
        Task<bool> AddTrackAsync(NewMusicTrackModelDTO musicTrackModel, string userId);

        // Add new liked track
        Task<bool> AddLikedTrackAsync(string trackId, string userId);

        // Return music track by id
        Task<MusicTrackModelDTO?> GetMusicTrackByIdAsync(string trackId);

        // Return all music tracks
        Task<IEnumerable<MusicTrackModelDTO>> GetAllMusicTracksAsync(string? userId = null);

        // Return list of user uploaded tracks
        Task<IEnumerable<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId);

        // Return all liked tracks by user id
        Task<IEnumerable<MusicTrackModelDTO>> GetAllLikedMusicTracksAsync(string userId);

        // Get music styles
        Task<IEnumerable<Style>> GetMusicStylesAsync();

        // Update music track
        Task<bool> UpdateMusicTrackAsync(string trackId, NewMusicTrackModelDTO musicTrackModel);

        // Delete music track
        Task<bool> DeleteMusicTrackAsync(string trackId);

        // Delete liked music track from author
        Task<bool> DeleteLikedTrackAsync(string userId, string trackId);
    }
}
