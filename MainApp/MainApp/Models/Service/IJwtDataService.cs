namespace MainApp.Models.Service
{
    public interface IJwtDataService
    {
        Task CheckUserRefreshTokensCountAsync(UserModel user);
        Task AddRefreshTokenAsync(string refreshToken, UserModel user);
        Task<string> GetRefreshTokenDataAsync(string userId);
        Task RemoveRefreshTokenDataAsync(string userId);
    }
}
