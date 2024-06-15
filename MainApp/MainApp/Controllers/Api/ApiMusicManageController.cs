using MainApp.Interfaces.Music;
using MainApp.Models.Music;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
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
        public async Task<IEnumerable<MusicTrack>> GetAllMusicTracks()
        {
            return await musicService.GetAllMusicTracksAsync();
        }


        [HttpGet("{musicTrackId}")]
        public async Task<MusicTrack?> SearchTrackByTitle(string musicTrackId)
        {
            return await musicService.GetMusicTrackByIdAsync<MusicTrack>(musicTrackId);
        }

        [HttpGet("styles")]
        public async Task<IEnumerable<Style>> GetAllMusicStyles()
        {
            return await musicService.GetMusicStylesAsync();
        }

        [HttpGet("filter/style/{styleId}")]
        public async Task<IEnumerable<MusicTrack>> FilterTracksByStyle(string styleId)
        {
            return (await musicService.GetAllMusicTracksAsync()).Where(m => m.Style.Id == styleId);
        }
    }
}
