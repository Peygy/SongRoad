using System.Security.Claims;

namespace MainApp.Interfaces.User
{
    public interface IUserService
    {
        // Get user roles
        List<string> GetUserRoles();
        // Get user identifier
        Task<string?> GetUserId();
    }
}
