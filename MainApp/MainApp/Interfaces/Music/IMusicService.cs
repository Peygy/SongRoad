using MainApp.DTO.Music;
using MainApp.Models.Music;
using MainApp.Models.User;

namespace MainApp.Interfaces.Music
{
    public interface IMusicService
    {
        // Check if author exist in database
        Task CheckAuthorExistAsync(UserModel user);

        // Add new uploaded track
        Task<bool> AddTrackAsync(NewMusicTrackModelDTO musicTrackModel, string userId);

        // Add new liked track
        Task<bool> AddLikedTrackAsync(string trackId, string userId);

        // Return list of user uploaded tracks
        Task<List<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId);

        // Return music track by id
        Task<MusicTrackModelDTO?> GetMusicTrackByIdAsync(string trackId);

        // Return all liked tracks by user id
        Task<List<MusicTrackModelDTO>> GetAllLikedMusicTracksAsync(string userId);

        // Return tracks for view
        Task<List<MusicTrackModelDTO>> GetMusicTracksForViewAsync(string? userId);

        // Return all music tracks
        Task<IEnumerable<MusicTrack>> GetAllMusicTracksAsync();

        // Get music styles
        Task<List<Style>> GetMusicStylesAsync();

        // Update music track
        Task UpdateMusicTrackAsync(string trackId, NewMusicTrackModelDTO musicTrackModel);

        // Delete music track
        Task<bool> DeleteMusicTrackAsync(string trackId);

        // Delete liked music track from author
        Task<bool> DeleteLikedTrackAsync(string userId, string trackId);
    }
}
