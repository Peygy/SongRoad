using MainApp.Interfaces.Music;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MainApp.Controllers
{
    [Route("/home")]
    public class HomeController : Controller
    {
        private readonly IMusicService musicService;

        public HomeController(IMusicService musicService)
        {
            this.musicService = musicService;
        }

        public async Task<IActionResult> Home()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tracks = await musicService.GetAllMusicTracksAsync(userId);
            return View(tracks.ToList());
        }
    }
}
