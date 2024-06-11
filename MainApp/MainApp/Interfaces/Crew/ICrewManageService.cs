using MainApp.Models.User;

namespace MainApp.Interfaces.Crew
{
    public interface ICrewManageService
    {
        // Adds a new moderator
        Task<bool> AddNewModerAsync(string moderId);
        // Retrieves a list of all moderators
        Task<IEnumerable<UserModel>> GetAllModersAsync();
        // Removes a user from the moderators
        Task<bool> RemoveModerAsync(string moderId);
    }
}
