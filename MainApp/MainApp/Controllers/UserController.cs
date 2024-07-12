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
            var userId = await EnsureUserIsAuthorAsync();
            if (userId != null)
            {
                if (musicTrackModel.Mp3File?.Length > 0)
                {
                    var result = await musicService.AddTrackAsync(musicTrackModel, userId);
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

            return BadRequest();
        }

        [HttpGet("tracks/update/{trackId}")]
        public async Task<IActionResult> UpdateTrack(string trackId)
        {
            var musicTrack = await musicService.GetMusicTrackByIdAsync(trackId);
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
            if (await musicService.UpdateMusicTrackAsync(trackId, musicTrackModel))
            {
                return RedirectToAction("Account", "User");
            }

            return BadRequest();
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
            var userId = await EnsureUserIsAuthorAsync();
            if (userId != null)
            {
                var userUploadedTracks = await musicService.GetUserUploadedTrackListAsync(userId);

                return View(userUploadedTracks);
            }

            return BadRequest();
        }

        [HttpGet("tracks/liked")]
        public async Task<IActionResult> UserLikedTracks()
        {
            var userId = await EnsureUserIsAuthorAsync();
            if (userId != null)
            {
                var userLikedTracks = await musicService.GetAllLikedMusicTracksAsync(userId);

                return View(userLikedTracks);
            }

            return BadRequest();
        }

        [HttpPost("tracks/like")]
        public async Task<bool> LikeMusicTrack(string trackId)
        {
            var userId = await EnsureUserIsAuthorAsync();
            if (userId != null)
            {
                return await musicService.AddLikedTrackAsync(trackId, userId);
            }

            return false;
        }

        [HttpPost("tracks/unlike")]
        public async Task<bool> UnlikeMusicTrack(string trackId)
        {
            var userId = await userService.GetUserId();
            if (userId != null)
            {
                return await musicService.DeleteLikedTrackAsync(userId, trackId);
            }

            return false;
        }

        private async Task<string?> EnsureUserIsAuthorAsync()
        {
            var user = await userService.GetUser();
            if (user != null)
            {
                await musicService.CheckAuthorExistAsync(user);
                return user.Id;
            }

            return null;
        }
    }
}
