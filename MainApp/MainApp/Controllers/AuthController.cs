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

                return StatusCode(500, "Пользователь с таким логином уже существует");
                // ViewBag.Error = "Пользователь с таким логином уже существует";
                // return View(newUser);
            }

            return StatusCode(500, "Model isn't valid");
            // return View(newUser);
        }



        // Account login
        // Переделать потом под /login
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

                return StatusCode(500, "Логин или пароль неверны!");
                // ViewBag.Error = "Логин или пароль неверны!";
                // return View(loginUser);
            }

            return StatusCode(500, "Model isn't valid");
            // return View(loginUser);
        }


        /*
        // Account logout
        public async Task<IActionResult> Logout()
        {
            await cookieService.LogoutAsync(HttpContext);
            return RedirectToAction("Welcome", "Page");
        }*/



        // Crew login
        /*[HttpGet]
        public IActionResult CrewLogin()
        {
            // По идее можем тупо чекнуть рефреш и все, типо если есть то обратно дректим на глав. страницу
            if (!Request.Cookies.ContainsKey("refresh_token"))
                return RedirectToAction("Welcome", "Page");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrewLogin(LoginModel admin)
        {
            if (ModelState.IsValid)
            {
                if(await authService.CrewAuthenticationAsync(admin, "admin"))
                {
                    await cookieService.CookieAuthenticateAsync(admin.Login, "admin", HttpContext);
                    return RedirectToAction("ViewParts", "Part", new { table = "sections" });
                }
                else if (await authService.CrewAuthenticationAsync(admin, "editor"))
                {
                    await cookieService.CookieAuthenticateAsync(admin.Login, "editor", HttpContext);
                    return RedirectToAction("ViewParts", "Part", new { table = "sections" });
                }

                ViewBag.Error = "Логин или пароль неверны!";
                return View(admin);
            }

            return View(admin);
        }*/
    }
}
