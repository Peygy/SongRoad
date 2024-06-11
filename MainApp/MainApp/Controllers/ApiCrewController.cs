using MainApp.Interfaces.Crew;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    /// <summary>
    /// Api controller for crew commands endpoints
    /// </summary>
    [Route("api/crew")]
    [ApiController]
    [Authorize(Roles = UserRoles.Moderator)]
    public class ApiCrewController : ControllerBase
    {
        private readonly IUserManageService userManageService;
        private readonly ICrewManageService crewManageService;

        public ApiCrewController(IUserManageService userManageService, ICrewManageService crewManageService)
        {
            this.userManageService = userManageService;
            this.crewManageService = crewManageService;
        }

        [HttpGet("user")]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            return await userManageService.GetAllUsersAsync();
        }

        [HttpGet("user/warn/{userId}")]
        public async Task<int> GetUserWarnCount(string userId)
        {
            return await userManageService.GetUserWarnCountAsync(userId);
        }
        [HttpPost("user/warn/{userId}")]
        public async Task<int> AddWarnToUser(string userId)
        {
            return await userManageService.AddWarnToUserAsync(userId);
        }

        [HttpGet("user/ban/{userId}")]
        public async Task<bool> GetUserBanned(string userId)
        {
            return await userManageService.GetUserBannedAsync(userId);
        }
        [HttpPost("user/ban/{userId}")]
        public async Task<bool> AddBanToUser(string userId)
        {
            return await userManageService.AddBanToUserAsync(userId);
        }
        [HttpPost("user/unban/{userId}")]
        public async Task<bool> UnBanToUser(string userId)
        {
            return await userManageService.UnBanToUserAsync(userId);
        }


        // For Admin
        [HttpPost("moder/{moderId}")]
        public async Task<bool> AddModer(string moderId)
        {
            return await crewManageService.AddNewModerAsync(moderId);
        }

        [HttpGet("moder")]
        public async Task<IEnumerable<UserModel>> GetAllModers()
        {
            return await crewManageService.GetAllModersAsync();
        }

        [HttpDelete("moder/{moderId}")]
        public async Task<bool> RemoveModer(string moderId)
        {
            return await crewManageService.RemoveModerAsync(moderId);
        }
    }
}
