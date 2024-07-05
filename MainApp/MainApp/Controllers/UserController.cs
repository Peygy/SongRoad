using MainApp.DTO.Music;
using MainApp.Interfaces.Music;
using MainApp.Interfaces.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller for user actions
    /// </summary>
    [Authorize(Roles = UserRoles.User)]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService userService;
        private readonly IMusicService musicService;

        public UserController(IUserService userService, IMusicService musicService)
        {
            this.userService = userService;
            this.musicService = musicService;
        }

        [HttpGet("account")]
        public IActionResult Account()
        {
            return View();
        }

        [HttpGet("tracks/add")]
        public async Task<IActionResult> AddTrack()
        {
            ViewBag.Styles = new SelectList(await musicService.GetMusicStylesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost("tracks/add")]
        public async Task<IActionResult> AddTrack(NewMusicTrackModelDTO musicTrackModel)
        {
            var user = await userService.GetUser();
            await musicService.CheckAuthorExistAsync(user);

            if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
            {
                var result = await musicService.AddTrackAsync(musicTrackModel, user.Id);
                if (result)
                {
                    return RedirectToAction("Account", "User");
                }
                ViewBag.ErrorMessage = "Трек с таким названием существует";
            }
            else
            {
                ViewBag.ErrorMessage = "Файл не был загружен.";
            }

            return View();
        }

        [HttpGet("tracks/update/{trackId}")]
        public async Task<IActionResult> UpdateTrack(string trackId)
        {
            var musicTrack = await musicService.GetMusicTrackByIdAsync<MusicTrackModelDTO>(trackId);
            if (musicTrack != null)
            {
                ViewBag.OldTrack = musicTrack;
                ViewBag.Styles = new SelectList(await musicService.GetMusicStylesAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.ErrorMessage = "Музыкальный трек не найден!";
            }

            return View();
        }

        [HttpPost("tracks/update/{trackId}")]
        public async Task<IActionResult> UpdateTrack(string trackId, NewMusicTrackModelDTO musicTrackModel)
        {
            await musicService.UpdateMusicTrackAsync(trackId, musicTrackModel);
            return RedirectToAction("Account", "User");
        }

        [HttpDelete("tracks/delete/{trackId}")]
        public async Task<IActionResult> DeleteTrack(string trackId)
        {
            if (await musicService.DeleteMusicTrackAsync(trackId))
            {
                return Ok(new { success = true });
            }

            return BadRequest(new { success = false });
        }

        [HttpGet("tracks/uploaded")]
        public async Task<IActionResult> UserUploadedTracks()
        {
            var user = await userService.GetUser();
            await musicService.CheckAuthorExistAsync(user);
            var userUploadedTracks = await musicService.GetUserUploadedTrackListAsync(user.Id);

            return View(userUploadedTracks);
        }

        [HttpGet("tracks/liked")]
        public async Task<IActionResult> UserLikedTracks()
        {
            var user = await userService.GetUser();
            await musicService.CheckAuthorExistAsync(user);
            var userLikedTracks = await musicService.GetAllLikedMusicTracksAsync(user.Id);

            return View(userLikedTracks);
        }

        [HttpPost("tracks/liked")]
        public async Task LikeMusicTrack(string trackId)
        {
            var user = await userService.GetUser();
            await musicService.CheckAuthorExistAsync(user);
            await musicService.AddLikedTrackAsync(trackId, user.Id);
        }
    }
}
