using MainApp.Models.Service;

namespace MainApp.Services
{
    // Service to generate cookies when logging in or registering
    // and deleting them when logging out of the site
    public class CookieService : ICookieService
    {
        // Logger for exceptions
        private readonly ILogger<CookieService> log;
        private readonly IHttpContextAccessor httpContext;

        public CookieService(ILogger<CookieService> log, IHttpContextAccessor httpContextAccessor)
        {
            this.log = log;
            this.httpContext = httpContextAccessor;
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
                var cookies = httpContext.HttpContext.Response.Cookies;
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
            return httpContext.HttpContext.Request.Cookies["access_token"];
        }

        public string? GetRefreshToken()
        {
            return httpContext.HttpContext.Request.Cookies["refresh_token"];
        }

        public void DeleteTokens()
        {
            if (httpContext.HttpContext.Request.Cookies.ContainsKey("access_token"))
            {
                httpContext.HttpContext.Response.Cookies.Append("access_token", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });

                httpContext.HttpContext.Response.Cookies.Append("refresh_token", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });
            }
        }
    }
}
