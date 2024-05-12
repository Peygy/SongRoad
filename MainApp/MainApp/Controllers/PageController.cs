using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    public class PageController : Controller
    {
        public IActionResult Welcome()
        {
            return View();
        }

        public IActionResult Home()
        {
            return View();
        }
    }
}
