using MainApp.Interfaces.Entry;

namespace MainApp.Services
{
    /// <summary>
    /// Class of service for generate cookies when logging in or registering and deleting them when logging out
    /// </summary>
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


        /// <summary>
        /// Set access and refresh tokens to cookies
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="refreshToken">Refresh token</param>
        public void SetTokens(string accessToken, string refreshToken)
        {
            SetCookie("access_token", accessToken);
            SetCookie("refresh_token", refreshToken, expires: DateTime.UtcNow.AddDays(30));
        }

        /// <summary>
        /// Method for setting cookies
        /// </summary>
        /// <param name="key">Key for cookie item</param>
        /// <param name="value">Value for cookie item</param>
        /// <param name="expires">Expired time for cookie item</param>
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

        /// <summary>
        /// Method for get access token
        /// </summary>
        /// <returns>Access token</returns>
        public string? GetAccessToken()
        {
            return httpContextAccessor.HttpContext.Request.Cookies["access_token"];
        }

        /// <summary>
        /// Method for get refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        public string? GetRefreshToken()
        {
            return httpContextAccessor.HttpContext.Request.Cookies["refresh_token"];
        }

        /// <summary>
        /// Delete access and refresh tokens from cookies
        /// </summary>
        public void DeleteTokens()
        {
            if (httpContextAccessor.HttpContext.Request.Cookies.ContainsKey("access_token"))
            {
                httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });

                httpContextAccessor.HttpContext.Response.Cookies.Append("refresh_token", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });
            }
        }
    }
}
