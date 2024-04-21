using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MainApp.Services;
using MainApp.Models;

namespace MainApp.Controllers
{
    // Controller to manage users and crew
    [Authorize(Roles = UserRoles.User)]
    public class CrewController : Controller
    {
        public string Check()
        {
            return "Access User";
        }

        public IActionResult ViewUsers()
        {
            return View("~/Views/Auth/UserLogin.cshtml");
        }
    }
}
