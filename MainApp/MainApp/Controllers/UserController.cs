using MainApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    [Authorize(Roles = UserRoles.User)]
    public class UserController : Controller
    {
        public IActionResult Account()
        {
            return View();
        }
    }
}
