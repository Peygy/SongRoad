using MainApp.DTO.Music;
using MainApp.Services.Music;
using MainApp.Models.Music;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    /// <summary>
    /// Api controller for managing music tracks by crew
    /// </summary>
    [Route("api/crew/music")]
    [ApiController]
    [Authorize(Roles = UserRoles.Moderator)]
    public class ApiMusicManageController : ControllerBase
    {
        private readonly IMusicService musicService;

        public ApiMusicManageController(IMusicService musicService)
        {
            this.musicService = musicService;
        }

        [HttpGet]
        public async Task<IEnumerable<MusicTrackModelDTO>> GetAllMusicTracks()
        {
            return await musicService.GetAllMusicTracksAsync();
        }

        [HttpGet("{musicTrackId}")]
        public async Task<MusicTrackModelDTO?> SearchTrackByTitle(string musicTrackId)
        {
            return await musicService.GetMusicTrackByIdAsync(musicTrackId);
        }

        [HttpGet("styles")]
        public async Task<IEnumerable<Style>> GetAllMusicStyles()
        {
            return await musicService.GetMusicStylesAsync();
        }

        [HttpGet("filter/style/{styleName}")]
        public async Task<IEnumerable<MusicTrackModelDTO>> FilterTracksByStyle(string styleName)
        {
            return (await musicService.GetAllMusicTracksAsync()).Where(m => m.Style == styleName);
        }

        [HttpDelete("{musicTrackId}")]
        public async Task<bool> DeleteMusicTrackById(string musicTrackId)
        {
            return await musicService.DeleteMusicTrackAsync(musicTrackId);
        }
    }
}
