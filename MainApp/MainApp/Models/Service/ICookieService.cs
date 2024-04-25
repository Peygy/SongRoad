namespace MainApp.Models.Service
{
    public interface ICookieService
    {
        void SetTokens(string accessToken, string refreshToken);

        string? GetAccessToken();
        string? GetRefreshToken();

        void DeleteTokens();
    }
}
