using MainApp.Interfaces.Crew;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    /// <summary>
    /// Api controller for crew commands endpoints
    /// </summary>
    [Route("api/crew/user")]
    [ApiController]
    [Authorize(Roles = UserRoles.Moderator)]
    public class ApiUserManageController : ControllerBase
    {
        private readonly IUserManageService userManageService;

        public ApiUserManageController(IUserManageService userManageService)
        {
            this.userManageService = userManageService;
        }

        [HttpGet]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            return await userManageService.GetAllUsersAsync();
        }

        [HttpGet("warn/{userId}")]
        public async Task<int> GetUserWarnCount(string userId)
        {
            return await userManageService.GetUserWarnCountAsync(userId);
        }
        [HttpPost("warn/{userId}")]
        public async Task<int> AddWarnToUser(string userId)
        {
            return await userManageService.AddWarnToUserAsync(userId);
        }

        [HttpGet("ban/{userId}")]
        public async Task<bool> GetUserBanned(string userId)
        {
            return await userManageService.GetUserBannedAsync(userId);
        }

        [HttpPost("ban/{userId}")]
        public async Task<bool> AddBanToUser(string userId)
        {
            return await userManageService.AddBanToUserAsync(userId);
        }

        [HttpPost("unban/{userId}")]
        public async Task<bool> UnBanToUser(string userId)
        {
            return await userManageService.UnBanToUserAsync(userId);
        }
    }
}
