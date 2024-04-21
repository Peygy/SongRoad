using MainApp.Models.Service;
using Microsoft.Extensions.Logging;

namespace MainApp.Services
{
    // Service to generate cookies when logging in or registering
    // and deleting them when logging out of the site
    public class CookieService : ICookieService
    {
        // Logger for exceptions
        private readonly ILogger<CookieService> log;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CookieService(ILogger<CookieService> log, IHttpContextAccessor httpContextAccessor)
        {
            this.log = log;
            this.httpContextAccessor = httpContextAccessor;
        }


        public void SetTokens(string accessToken, string refreshToken)
        {
            SetAccessToken(accessToken);
            SetRefreshToken(refreshToken);
        }
        private void SetAccessToken(string accessToken)
        {
            try
            {
                var cookies = httpContextAccessor.HttpContext.Response.Cookies;

                cookies.Delete("access_token");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true
                };
                cookies.Append("access_token", accessToken, cookieOptions);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }
        private void SetRefreshToken(string refreshToken)
        {
            try
            {
                var cookies = httpContextAccessor.HttpContext.Response.Cookies;

                cookies.Delete("refresh_token");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(30)
                };
                cookies.Append("refresh_token", refreshToken, cookieOptions);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public string? GetAccessToken()
        {
            return httpContextAccessor.HttpContext.Request.Cookies["access_token"];
        }
        public string? GetRefreshToken()
        {
            return httpContextAccessor.HttpContext.Request.Cookies["refresh_token"];
        }
    }
}
