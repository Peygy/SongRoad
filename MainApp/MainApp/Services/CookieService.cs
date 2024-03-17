using MainApp.Models.Service;

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
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true
            };
            httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", accessToken, cookieOptions);
        }
        private void SetRefreshToken(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(30)
            };
            httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
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
