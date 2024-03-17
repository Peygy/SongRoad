namespace MainApp.Models.Service
{
    public interface ICookieService
    {
        void SetTokens(string accessToken, string refreshToken);
    }
}
