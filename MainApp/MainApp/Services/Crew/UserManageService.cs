using MainApp.Data;
using MainApp.Interfaces.Crew;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Services.Crew
{
    /// <summary>
    /// Class for manage crew actions over users
    /// </summary>
    [Authorize(Roles = UserRoles.Moderator)]
    public class UserManageService : IUserManageService
    {
        private readonly UserManager<UserModel> userManager;
        private readonly UserContext userContext;

        public UserManageService(UserManager<UserModel> userManager, UserContext userContext)
        {
            this.userManager = userManager;
            this.userContext = userContext;
        }

        /// <summary>
        /// Method for get all users
        /// </summary>
        /// <returns>Collection of users</returns>
        public async Task<IEnumerable<UserModel>> GetAllUsersAsync()
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

        /// <summary>
        /// Method for get user warning, e.x if bad music track
        /// </summary>
        /// <param name="userId">Choice user's id</param>
        /// <returns>Count of user's warnings</returns>
        public async Task<int> GetUserWarnCountAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                return 0;
            }

            return userRights.WarnCount;
        }
        /// <summary>
        /// Method for add warning to user
        /// </summary>
        /// <param name="userId">Choice user's id</param>
        /// <returns>Count of user's warnings</returns>
        public async Task<int> AddWarnToUserAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                userRights = new UserRights { UserId = userId, WarnCount = 1 };
                await userContext.UserRights.AddAsync(userRights);
            }
            else
            {
                userRights.WarnCount++;
                // If user has 3 warning -> will be banned
                if (userRights.WarnCount == 3)
                {
                    await BanUserAsync(userId);
                }
            }

            await userContext.SaveChangesAsync();
            return userRights.WarnCount;
        }

        /// <summary>
        /// Method for get state of user ban
        /// </summary>
        /// <param name="userId">Choice user's id</param>
        /// <returns>Ban status</returns>
        public async Task<bool> GetUserBannedAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                return false;
            }

            return userRights.Banned;
        }
        /// <summary>
        /// Method for add ban to user
        /// </summary>
        /// <param name="userId">Choice user's id</param>
        /// <returns>Ban status</returns>
        public async Task<bool> AddBanToUserAsync(string userId)
        {
            return await BanUserAsync(userId);
        }
        /// <summary>
        /// Method for unban user
        /// </summary>
        /// <param name="userId">Choice user's id</param>
        /// <returns>Unban status</returns>
        public async Task<bool> UnBanToUserAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights != null)
            {
                userRights.WarnCount = 0;
                userRights.Banned = false;
                await userContext.SaveChangesAsync();
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await userManager.AddToRoleAsync(user, UserRoles.User);
                return result.Succeeded;
            }

            return false;
        }

        /// <summary>
        /// Method for banning user
        /// </summary>
        /// <param name="userId">Choice user's id</param>
        /// <returns>Result of banning</returns>
        private async Task<bool> BanUserAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                userRights = new UserRights { UserId = userId, Banned = true };
                await userContext.UserRights.AddAsync(userRights);
            }
            else
            {
                userRights.Banned = true;
            }
            await userContext.SaveChangesAsync();

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
