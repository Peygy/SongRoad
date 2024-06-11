using MainApp.Interfaces.Crew;
using MainApp.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MainApp.Services.Crew
{
    /// <summary>
    /// Class for manage crew actions over other crews (admins over moders)
    /// </summary>
    [Authorize(Roles = UserRoles.Admin)]
    public class CrewManageService : ICrewManageService
    {
        private readonly UserManager<UserModel> userManager;

        public CrewManageService(UserManager<UserModel> userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>
        /// Method for add new moderator - add moder role
        /// </summary>
        /// <param name="moderId">Choice moder's id</param>
        /// <returns>Result of adding new moderator</returns>
        public async Task<bool> AddNewModerAsync(string moderId)
        {
            var user = await userManager.FindByIdAsync(moderId);
            var result = await userManager.AddToRoleAsync(user, UserRoles.Moderator);
            return result.Succeeded;
        }

        /// <summary>
        /// Method for get all moderators
        /// </summary>
        /// <returns>Collection of all moderators</returns>
        public async Task<IEnumerable<UserModel>> GetAllModersAsync()
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

        /// <summary>
        /// Method for remove moderator - remove moder role
        /// </summary>
        /// <param name="moderId">Choice moder's id</param>
        /// <returns>Result of removing moderator</returns>
        public async Task<bool> RemoveModerAsync(string moderId)
        {
            var user = await userManager.FindByIdAsync(moderId);
            var result = await userManager.RemoveFromRoleAsync(user, UserRoles.Moderator);
            return result.Succeeded;
        }
    }
}
