using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MainApp.Models;
using MainApp.Models.Service;

namespace MainApp.Controllers
{
    // Controller to manage users and crew
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