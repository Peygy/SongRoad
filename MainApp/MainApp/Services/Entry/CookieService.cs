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
            SetCookie("access_token", accessToken);
            SetCookie("refresh_token", refreshToken, expires: DateTime.UtcNow.AddDays(30));
        }

        private void SetCookie(string key, string value, DateTime? expires = null)
        {
            try
            {
                var cookies = httpContextAccessor.HttpContext.Response.Cookies;
                var cookieOptions = new CookieOptions { HttpOnly = true, Expires = expires };
                cookies.Append(key, value, cookieOptions);
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
