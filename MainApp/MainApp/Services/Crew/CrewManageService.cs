using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MainApp.Services.Crew
{
    /// <summary>
    /// Service for performing actions over crew.
    /// </summary>
    public interface ICrewManageService
    {
        /// <summary>
        /// Add moderator role to user, who has specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">Specific identification of user</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing the state of the add operation.
        /// </returns>
        Task<bool> AddNewModerAsync(string userId);
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserModel>> GetAllModersAsync();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moderId"></param>
        /// <returns></returns>
        Task<bool> RemoveModerAsync(string moderId);
    }

    /// <inheritdoc cref="ICrewManageService">
    [Authorize(Roles = UserRoles.Admin)]
    public class CrewManageService : ICrewManageService
    {
        private readonly UserManager<UserModel> userManager;

        /// <summary>
        /// Constructs a new instance of <see cref="CrewManageService"/>.
        /// </summary>
        /// <param name="userManager"></param>
        public CrewManageService(UserManager<UserModel> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<bool> AddNewModerAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            var result = await userManager.AddToRoleAsync(user, UserRoles.Moderator);
            return result.Succeeded;
        }

        public async Task<IEnumerable<UserModel>> GetAllModersAsync()
        {
            var users = userManager.Users.ToList();
            for (int i = 0; i < users.Count; i++)
            {
                var roles = await userManager.GetRolesAsync(users[i]);
                if (!roles.Contains(UserRoles.Moderator) || roles.Contains(UserRoles.Admin))
                {
                    users.Remove(users[i]);
                    i--;
                }
            }

            return users;
        }

        public async Task<bool> RemoveModerAsync(string moderId)
        {
            var user = await userManager.FindByIdAsync(moderId);
            var result = await userManager.RemoveFromRoleAsync(user, UserRoles.Moderator);
            return result.Succeeded;
        }
    }
}
