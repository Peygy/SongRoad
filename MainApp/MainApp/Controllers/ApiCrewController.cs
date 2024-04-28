using MainApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Controllers
{
    [Route("api/crew")]
    [ApiController]
    [Authorize(Roles = UserRoles.Moderator)]
    public class ApiCrewController : ControllerBase
    {
        private readonly UserManager<UserModel> userManager;

        public ApiCrewController(UserManager<UserModel> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet("user")]
        public IEnumerable<UserModel> GetUsers()
        {
            return userManager.Users.ToList();
        }
        [HttpGet("user/roles/{userId}")]
        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            return await userManager.GetRolesAsync(user);
        }

        [HttpPost("user/warn")]
        public void AddWarnToUser(string value)
        {

        }
        [HttpPost("user/ban")]
        public void AddBanToUser(string value)
        {

        }
    }
}
