using Microsoft.AspNetCore.Mvc;
using MainApp.Models;

namespace MainApp.Controllers
{
    // Controller for managing user and staff registrations and logins
    public class AuthController : Controller
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }


        // Account registration
        [HttpGet]
        public IActionResult Registration() => View();

        [HttpPost]
        public async Task<IActionResult> Registration(RegisterModel newUser)
        {
            if (ModelState.IsValid)
            {
                if (await authService.UserRegister(newUser))
                {
                    return RedirectToAction("Check", "Crew");
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
                return RedirectToAction("Welcome", "Page");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel loginUser)
        {
            if (ModelState.IsValid)
            {
                if (await authService.UserLogin(loginUser))
                {
                    return RedirectToAction("Check", "Crew");
                }

                ViewBag.Error = "Логин или пароль неверны!";
                return View(loginUser);
            }

            return View(loginUser);
        }


        // Account logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.ContainsKey("refresh_token"))
                await authService.Logout();
            return RedirectToAction("Welcome", "Page");
        }
    }
}
