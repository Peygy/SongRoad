namespace MainApp.Interfaces.Entry
{
    public interface ICookieService
    {
        // Set access nad refresh tokens to storages
        void SetTokens(string accessToken, string refreshToken);

        // Get access token from cookies
        string? GetAccessToken();
        // Get refresh token from cookies
        string? GetRefreshToken();

        // Delete all tokens for current session
        void DeleteTokens();
    }
}
