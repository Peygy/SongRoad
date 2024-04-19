namespace MainApp.Models
{
    public interface IAuthService
    {
        // Register new user
        Task<bool> UserRegister(RegisterModel newUser);

        // User login
        Task<bool> UserLogin(LoginModel newUser);
        // Moderator login
        Task<bool> ModeratorLogin(LoginModel newUser);
        // Admin login
        Task<bool> AdminLogin(LoginModel newUser);
    }
}
