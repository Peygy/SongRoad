using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Account()
        {
            return View();
        }
    }
}
