using MainApp.Models;
using MainApp.Models.Data;
using MainApp.Models.Service;
using MainApp.Services;
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
        private readonly UserContext userContext;

        public ApiCrewController(UserManager<UserModel> userManager, UserContext userContext)
        {
            this.userManager = userManager;
            this.userContext = userContext;
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

        [HttpGet("user/warn/{userId}")]
        public async Task<int> GetUserWarnCount(string userId)
        {
            var userRights = await userContext.UserRights.FirstAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                await userContext.UserRights.AddAsync(new UserRights { UserId = userId });
                await userContext.SaveChangesAsync();
                return 0;
            }

            return userRights.WarnCount;
        }
        [HttpPost("user/warn/{userId}")]
        public async Task<int> AddWarnToUser(string userId)
        {
            var userRights = await userContext.UserRights.FirstAsync(x => x.UserId == userId);
            userRights.WarnCount++;
            if (userRights.WarnCount == 3)
            {
                await BanUser(userId);
            }

            await userContext.SaveChangesAsync();
            return userRights.WarnCount;
        }

        [HttpGet("user/ban/{userId}")]
        public async Task<bool> GetUserBanned(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                await userContext.UserRights.AddAsync(new UserRights { UserId = userId });
                await userContext.SaveChangesAsync();
                return false;
            }

            return userRights.Banned;
        }
        [HttpPost("user/ban/{userId}")]
        public async Task<bool> AddBanToUser(string userId)
        {
            return await BanUser(userId);
        }
        [HttpPost("user/unban/{userId}")]
        public async Task<bool> UnBanToUser(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            userRights.WarnCount = 0;
            userRights.Banned = false;
            await userContext.SaveChangesAsync();

            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.AddToRoleAsync(user, UserRoles.User);
            return result.Succeeded;
        }

        private async Task<bool> BanUser(string userId)
        {
            var userRights = await userContext.UserRights.FirstAsync(x => x.UserId == userId);
            userRights.Banned = true;

            var refreshTokenData = await userContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            refreshTokenData.TokensWhiteList.Clear();
            await userContext.SaveChangesAsync();

            var user = await userManager.FindByIdAsync(userId);
            var userRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, userRoles);

            return userRights.Banned;
        }
    }
}
