using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MainApp.Models;

namespace MainApp.Controllers
{
    // Controller to manage users and crew
    [Authorize(Roles = UserRoles.Moderator)]
    public class CrewController : Controller
    {
        [HttpGet]
        public IActionResult Control()
        {
            return View();
        }
    }
}