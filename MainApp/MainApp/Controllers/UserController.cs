using MainApp.DTO.Music;
using MainApp.Interfaces.Music;
using MainApp.Interfaces.User;
using MainApp.Models.Music;
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

        [HttpGet]
        public IActionResult Account()
        {
            return View();
        }

        [HttpGet("tracks")]
        public async Task<IActionResult> UserPersonalTracks()
        {
            var userId = await userService.GetUserId();
            var userPersonalTracks = await musicService.GetUserUploadedTrackListAsync(userId);

            return View(userPersonalTracks);
        }

        [HttpGet("download/file/music")]
        public async Task<IActionResult> DownloadMusicFile(string fileId)
        {
            var fileStream = await musicService.GetMusicTrackStreamAsync(fileId);
            if (fileStream == null)
            {
                return NotFound();
            }

            // Get file length
            long fileLength = fileStream.Length;
            string contentRange = $"bytes 0-{fileLength - 1}/{fileLength}";

            var response = File(fileStream, "audio/mpeg", fileId);

            // Add headers for correct file work
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Length", fileLength.ToString());
            Response.Headers.Add("Content-Range", contentRange);


            return response;
        }

        [HttpGet("tracks/liked")]
        public async Task<IActionResult> UserLikedTracks()
        {
            var userId = await userService.GetUserId();
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
            var userId = await userService.GetUserId();

            if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
            {
                await musicService.AddTrackAsync(musicTrackModel, userId);
                return RedirectToAction("Account", "User");
            }
            else
            {
                ViewBag.ErrorMessage = "Файл не был загружен.";
                return View();
            }
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
            await musicService.UpdateMusicTrackAsync(trackId, musicTrackModel);
            return RedirectToAction("Account", "User");
        }
    }
}
