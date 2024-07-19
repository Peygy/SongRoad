using MainApp.DTO.Music;
using MainApp.Services.Music;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller for user actions
    /// </summary>
    [Authorize(Roles = UserRoles.User)]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IMusicService musicService;

        public UserController(IMusicService musicService)
        {
            this.musicService = musicService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet("account")]
        public async Task<IActionResult> Account()
        {
            if (TempData["RedirectedFromAuth"] != null)
            {
                var user = new UserModel
                {
                    Id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                    UserName = User.FindFirstValue(ClaimTypes.Name)
                };
                await musicService.CheckAuthorExistAsync(user);
            }

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
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                if (musicTrackModel.Mp3File?.Length > 0)
                {
                    var result = await musicService.AddTrackAsync(musicTrackModel, userId);
                    if (result)
                    {
                        return RedirectToAction("UserUploadedTracks");
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
                ViewBag.Styles = await musicService.GetMusicStylesAsync();
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
                return RedirectToAction("UserUploadedTracks");
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
            var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                return await musicService.AddLikedTrackAsync(trackId, userId);
            }

            return false;
        }

        [HttpPost("tracks/unlike")]
        public async Task<bool> UnlikeMusicTrack(string trackId)
        {
            var userId = GetCurrentUserId();
            if (userId != null)
            {
                return await musicService.DeleteLikedTrackAsync(userId, trackId);
            }

            return false;
        }
    }
}
