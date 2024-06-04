using MainApp.Data;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly UserManager<UserModel> userManager;
        private readonly UserContext userContext;

        public ApiCrewController(UserManager<UserModel> userManager, UserContext userContext)
        {
            this.userManager = userManager;
            this.userContext = userContext;
        }

        [HttpGet("user")]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            var users = userManager.Users.ToList();
            for (int i = 0; i < users.Count; i++)
            {
                var roles = await userManager.GetRolesAsync(users[i]);
                if (roles.Contains(UserRoles.Moderator) || roles.Contains(UserRoles.Admin))
                {
                    users.Remove(users[i]);
                    i--;
                }
            }

            return users;
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


        // For Admin
        [Authorize(Roles = UserRoles.Admin)]
        [HttpPost("moder/{userId}")]
        public async Task<bool> AddModer(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.AddToRoleAsync(user, UserRoles.Moderator);
            return result.Succeeded;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet("moder")]
        public async Task<IEnumerable<UserModel>> GetAllModers()
        {
            var users = userManager.Users.ToList();
            for (int i = 0; i < users.Count; i++)
            {
                var roles = await userManager.GetRolesAsync(users[i]);
                if (roles.Contains(UserRoles.User) && roles.Count != 2 || roles.Contains(UserRoles.Admin))
                {
                    users.Remove(users[i]);
                    i--;
                }
            }

            return users;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("moder/{userId}")]
        public async Task<bool> RemoveModer(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.RemoveFromRoleAsync(user, UserRoles.Moderator);
            return result.Succeeded;
        }
    }
}
