using Microsoft.AspNetCore.Mvc;
using MainApp.DTO.User;
using MainApp.Services.Entry;

namespace MainApp.Controllers
{
    /// <summary>
    /// Controller for managing user and staff registrations and logins
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }


        // Account registration (User)
        [HttpGet]
        public IActionResult Registration()
        {
            if (Request.Cookies.ContainsKey("refresh_token"))
                return RedirectToAction("Account", "User");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegisterModelDTO newUser)
        {
            if (ModelState.IsValid)
            {
                if (await authService.UserRegister(newUser))
                {
                    TempData["RedirectedFromAuth"] = true;
                    return RedirectToAction("Account", "User");
                }

                ViewBag.Error = "Пользователь с таким логином уже существует";
                return View(newUser);
            }

            return View(newUser);
        }


        // Account login
        [HttpGet]
        public IActionResult Login()
        {
            if (Request.Cookies.ContainsKey("refresh_token"))
                return RedirectToAction("Account", "User");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModelDTO loginUser)
        {
            if (ModelState.IsValid)
            {
                if (await authService.UserLogin(loginUser))
                {
                    TempData["RedirectedFromAuth"] = true;
                    return RedirectToAction("Account", "User");
                }

                ViewBag.Error = "Логин или пароль неверны!";
                return View(loginUser);
            }

            return View(loginUser);
        }


        // Account logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.ContainsKey("refresh_token"))
            {
                var resultOfLogout = await authService.Logout();
                if (resultOfLogout)
                {
                    return RedirectToAction("Welcome", "Page");
                }
            }

            ViewBag.Error = "Что-то пошло не так...";
            return View();
        }
    }
}
