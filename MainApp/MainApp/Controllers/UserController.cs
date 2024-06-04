using MainApp.Interfaces.Music;
using MainApp.Interfaces.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MainApp.DTO.Music;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller for user actions
    /// </summary>
    [Authorize(Roles = UserRoles.User)]
    [Route("{action}")]
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

        [HttpGet]
        public async Task<IActionResult> AddTrack()
        {
            ViewBag.Styles = new SelectList(await musicService.GetMusicStylesAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTrack(MusicTrackModelDTO musicTrackModel)
        {
            var userId = await userService.GetUserId();

            if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
            {
                await musicService.AddTrackAsync(musicTrackModel, userId);
                return RedirectToAction("Home", "Page");
            }
            else
            {
                ViewBag.ErrorMessage = "Файл не был загружен.";
                return View();
            }
        }
    }
}
