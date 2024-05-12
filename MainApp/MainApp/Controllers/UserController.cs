using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
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
        public IActionResult AddTrack()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTrack(string title, string style, IFormFile mp3File)
        {
            var userId = await userService.GetUserId();

            if (mp3File != null && mp3File.Length > 0)
            {
                await musicService.AddUserTrack(title, style, mp3File, userId);
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
