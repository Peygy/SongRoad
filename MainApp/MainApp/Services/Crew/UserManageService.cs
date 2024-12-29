using MainApp.Data;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Services.Crew
{
    /// <summary>
    /// Defines the contract for a service that manages user-related operations, such as retrieving users, 
    /// managing warnings, and handling bans.
    /// </summary>
    public interface IUserManageService
    {
        /// <summary>
        /// Retrieves all users from the system.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation,
        /// that contains an enumerable collection of users.
        /// </returns>
        Task<IEnumerable<UserModel>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves the number of warnings associated with a specific user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// that contains the number of warnings for the user.
        /// </returns>
        Task<int> GetUserWarnCountAsync(string userId);

        /// <summary>
        /// Adds a warning to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// that contains the updated number of warnings for the user.
        /// </returns>
        Task<int> AddWarnToUserAsync(string userId);

        /// <summary>
        /// Checks if a specific user is banned.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// that is <c>true</c> if the user is banned; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> GetUserBannedAsync(string userId);

        /// <summary>
        /// Bans the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// that is <c>true</c> if the ban was successfully applied; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> AddBanToUserAsync(string userId);

        /// <summary>
        /// Removes the ban from the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// that is <c>true</c> if the ban was successfully removed; otherwise, <c>false</c>.
        /// </returns>
        Task<bool> UnBanToUserAsync(string userId);
    }

    [Authorize(Roles = UserRoles.Moderator)]
    public class UserManageService : IUserManageService
    {
        /// <summary>
        /// Provides management and operation functionality for user accounts.
        /// </summary>
        private readonly UserManager<UserModel> userManager;
        /// <summary>
        /// The database context used for accessing user data.
        /// </summary>
        private readonly UserContext userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManageService"/> class.
        /// </summary>
        /// <param name="userManager">Provides management and operation functionality for user accounts.</param>
        /// <param name="userContext">The database context used for accessing user data.</param>
        public UserManageService(UserManager<UserModel> userManager, UserContext userContext)
        {
            this.userManager = userManager;
            this.userContext = userContext;
        }

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

        public async Task<int> GetUserWarnCountAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                return 0;
            }

            return userRights.WarnCount;
        }

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
                    var result = await BanUserAsync(userId);
                    if (!result)
                    {
                        return -1;
                    }
                }
            }

            await userContext.SaveChangesAsync();
            return userRights.WarnCount;
        }

        public async Task<bool> GetUserBannedAsync(string userId)
        {
            var userRights = await userContext.UserRights.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userRights == null)
            {
                return false;
            }

            return userRights.Banned;
        }

        public async Task<bool> AddBanToUserAsync(string userId)
        {
            return await BanUserAsync(userId);
        }

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
            if (refreshTokenData != null)
            {
                refreshTokenData.TokensWhiteList.Clear();
                await userContext.SaveChangesAsync();

                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var userRoles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, userRoles);
                }
            }

            return userRights.Banned;
        }
    }
}
