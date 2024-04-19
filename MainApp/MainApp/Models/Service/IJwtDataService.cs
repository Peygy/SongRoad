namespace MainApp.Models.Service
{
    public interface IJwtDataService
    {
        Task CheckUserRefreshTokensCountAsync(UserModel user);
        Task AddRefreshTokenAsync(string refreshToken, UserModel user);
    }
}
