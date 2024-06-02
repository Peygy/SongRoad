using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller for managing pages(views) of web service
    /// </summary>
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
