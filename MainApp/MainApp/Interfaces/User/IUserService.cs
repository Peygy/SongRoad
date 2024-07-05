using MainApp.Models.User;

namespace MainApp.Interfaces.User
{
    public interface IUserService
    {
        // Get user model
        Task<UserModel?> GetUser();
        // Get user roles
        List<string> GetUserRoles();
        // Get user identifier
        Task<string?> GetUserId();
    }
}
