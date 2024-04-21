namespace MainApp.Models.Service
{
    public interface IJwtDataService
    {
        Task CheckUserRefreshTokensCountAsync(UserModel user);
        Task AddTokensToStoragesAsync((string, string) tokens, UserModel user);
        Task<string> GetRefreshTokenAsync(string userId);
    }
}
