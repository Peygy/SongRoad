namespace MainApp.Services.Entry
{
    /// <summary>
    /// Defines the contract for a service that interacts with cookies.
    /// </summary>
    public interface ICookieService
    {
        /// <summary>
        /// Sets <paramref name="accessToken"/> and <paramref name="refreshToken"/> authentication tokens
        /// to <c>HttpOnly</c> cookies.
        /// </summary>
        /// <param name="accessToken">Jwt access token.</param>
        /// <param name="refreshToken">Refresh token.</param>
        void SetTokens(string accessToken, string refreshToken);

        /// <summary>
        /// Gets the jwt access token.
        /// </summary>
        /// <returns>
        /// The jwt access token from <c>HttpOnly</c> cookies.
        /// </returns>
        string? GetAccessToken();
        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <returns>
        /// The refresh token from <c>HttpOnly</c> cookies.
        /// </returns>
        string? GetRefreshToken();

        /// <summary>
        /// Deletes jwt access and refresh tokens from <c>HttpOnly</c> cookies,
        /// if tokens are exist.
        /// </summary>
        void DeleteTokens();
    }

    public class CookieService : ICookieService
    {
        /// <summary>
        /// The logger instance used for logging messages related to the <see cref="CookieService"/>.
        /// </summary>
        private readonly ILogger<CookieService> logger;
        /// <summary>
        /// Provides access to the current HTTP context, including request and response details.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieService"/> class.
        /// </summary>
        /// <param name="log">The logger instance used 
        /// for logging messages related to the <see cref="CookieService"/>.</param>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context, 
        /// including request and response details.
        /// </param>
        public CookieService(ILogger<CookieService> log, IHttpContextAccessor httpContextAccessor)
        {
            this.logger = log;
            this.httpContextAccessor = httpContextAccessor;
        }

        public void SetTokens(string accessToken, string refreshToken)
        {
            SetCookie("access_token", accessToken);
            SetCookie("refresh_token", refreshToken, expires: DateTime.UtcNow.AddDays(30));
        }

        /// <summary>
        /// Sets a <c>HttpOnly</c> cookie with expired time, if necessary.
        /// </summary>
        /// <param name="key">Key of a cookie.</param>
        /// <param name="value">Value of a cookie.</param>
        /// <param name="expires">Expire time of a cookie.</param>
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
                logger.LogError(ex.ToString());
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

        public void DeleteTokens()
        {
            if (httpContextAccessor.HttpContext.Request.Cookies.ContainsKey("access_token"))
            {
                httpContextAccessor.HttpContext.Response.Cookies
                    .Append("access_token", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });

                httpContextAccessor.HttpContext.Response.Cookies
                    .Append("refresh_token", "", new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                });
            }
        }
    }
}
