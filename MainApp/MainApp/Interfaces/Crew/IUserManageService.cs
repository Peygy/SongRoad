using MainApp.Models.User;

namespace MainApp.Interfaces.Crew
{
    public interface IUserManageService
    {
        // Retrieves a list of all users, excluding moderators and administrators
        Task<IEnumerable<UserModel>> GetAllUsersAsync();
        // Gets the warning count for a specified user
        Task<int> GetUserWarnCountAsync(string userId);
        // Adds a warning to the user and bans them if they reach 3 warnings
        Task<int> AddWarnToUserAsync(string userId);
        // Checks if the user is banned
        Task<bool> GetUserBannedAsync(string userId);
        // Bans the user
        Task<bool> AddBanToUserAsync(string userId);
        // Unbans the user
        Task<bool> UnBanToUserAsync(string userId);
    }
}
