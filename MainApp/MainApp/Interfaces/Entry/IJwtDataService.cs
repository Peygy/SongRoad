using MainApp.Models;

namespace MainApp.Interfaces.Entry
{
    public interface IJwtDataService
    {
        // Check user count of refresh tokens in db
        Task CheckUserRefreshTokensCountAsync(UserModel user);
        // Add new user refresh token to db
        Task AddRefreshTokenAsync(string refreshToken, UserModel user);
        // Get user refresh token from db
        Task<string> GetRefreshTokenDataAsync(string userId);
        // Remove refresh token from db
        Task RemoveRefreshTokenDataAsync(string userId);
    }
}
