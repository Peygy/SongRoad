using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MainApp.Models.User;
using MainApp.Interfaces.User;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller to manage users and crew
    /// </summary>
    [Authorize(Roles = UserRoles.Moderator)]
    public class CrewController : Controller
    {
        private readonly IUserService crewService;

        public CrewController(IUserService crewService)
        {
            this.crewService = crewService;
        }

        [HttpGet]
        public IActionResult Control()
        {
            ViewBag.Roles = crewService.GetUserRoles();
            return View();
        }
    }
}