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
        public IActionResult Check()
        {
            return Ok("Access root");
        }

        public IActionResult ViewUsers()
        {
            return View("~/Views/Auth/UserLogin.cshtml");
        }

        /*
        private CrewService crewService;

        public CrewController(UserContext userData, ILogger<CrewService> log)
        {
            // Data context for users -> userData
            // Logger for exceptions -> log
            crewService = new CrewService(userData, log);
        }



        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ViewUsers()
            => View(await crewService.GetUsersAsync());

        [HttpGet]
        [Authorize(Roles = "editor")]
        public async Task<IActionResult> ViewAdmins()
            => View(await crewService.GetAdminsAsync());



        [HttpGet]
        [Authorize(Roles = "editor")]
        public IActionResult AddAdmin()
            => View();

        [HttpPost]
        [Authorize(Roles = "editor")]
        public async Task<IActionResult> AddAdmin(Admin newAdmin)
        {
            if (ModelState.IsValid)
            {
                if (await crewService.AddAdminAsync(newAdmin))
                    return RedirectToAction("ViewAdmins");
                return RedirectToAction("Error", "Page");
            }

            return View(newAdmin);
        }



        [HttpDelete]
        [Route("api/users/remove/{id:int}")]
        [Authorize(Roles = "admin, editor")]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            var user = await crewService.RemoveUserAsync(id);
            if (user != null) return Json(user);
            return View();
        }

        [HttpDelete]
        [Route("api/admins/remove/{id:int}")]
        [Authorize(Roles = "editor")]
        public async Task<IActionResult> DeleteAdmins(int id)
        {
            var admin = await crewService.RemoveAdminAsync(id);
            if (admin != null) return Json(admin);
            return View();
        }
        */
    }
}
