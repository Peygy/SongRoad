using MainApp.Interfaces.Music;
using MainApp.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    [Route("/home")]
    public class HomeController : Controller
    {
        private readonly IUserService userService;
        private readonly IMusicService musicService;

        public HomeController(IUserService userService, IMusicService musicService)
        {
            this.userService = userService;
            this.musicService = musicService;
        }

        public async Task<IActionResult> Home()
        {
            var userId = await userService.GetUserId();
            var tracks = await musicService.GetMusicTracksForViewAsync(userId);
            return View(tracks.ToList());
        }
    }
}
