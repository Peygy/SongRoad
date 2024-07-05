using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller for managing no roots pages(views) of web service
    /// </summary>
    public class PageController : Controller
    {
        public IActionResult Welcome()
        {
            return View();
        }
    }
}
