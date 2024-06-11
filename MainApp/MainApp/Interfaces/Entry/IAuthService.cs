using MainApp.DTO.User;
using MainApp.Models.User;
using System.Security.Claims;

namespace MainApp.Interfaces.Entry
{
    public interface IAuthService
    {
        // Register new user
        Task<bool> UserRegister(RegisterModelDTO newUser);

        // User login
        Task<bool> UserLogin(LoginModelDTO newUser);
        // Moderator login
        Task<bool> ModeratorLogin(LoginModelDTO newUser);
        // Admin login
        Task<bool> AdminLogin(LoginModelDTO newUser);

        // Logout
        Task Logout();
    }
}