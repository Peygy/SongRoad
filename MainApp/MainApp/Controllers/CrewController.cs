using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MainApp.Models.User;
using System.Security.Claims;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller to manage users and crew
    /// </summary>
    [Authorize(Roles = UserRoles.Moderator)]
    public class CrewController : Controller
    {
        [HttpGet]
        public IActionResult Control()
        {
            ViewBag.Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            return View();
        }
    }
}