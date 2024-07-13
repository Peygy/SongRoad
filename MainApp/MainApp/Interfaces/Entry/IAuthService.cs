using MainApp.DTO.User;

namespace MainApp.Interfaces.Entry
{
    public interface IAuthService
    {
        // Register new user
        Task<bool> UserRegister(RegisterModelDTO newUser);

        // User login
        Task<bool> UserLogin(LoginModelDTO newUser);

        // Logout
        Task Logout();
    }
}