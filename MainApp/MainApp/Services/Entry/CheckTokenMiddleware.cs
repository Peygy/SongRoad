using MainApp.Models.User;
using System.Security.Claims;
using MainApp.Interfaces.Entry;

namespace MainApp.Services
{
    /// <summary>
    /// Middleware for check access and refersh tokens
    /// </summary>
    public class CheckTokenMiddleware
    {
        private readonly RequestDelegate next;

        public CheckTokenMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICookieService cookieService, IJwtGenService jwtGenService, IJwtDataService jwtDataService)
        {
            // Get access token from cookies
            var accessToken = cookieService.GetAccessToken();
            // Check access token
            if (accessToken != null && !jwtGenService.ValidAccessToken(accessToken))
            {
                // Get user data from jwt token
                var claims = jwtGenService.GetTokenUserClaims(accessToken).Claims;
                // Get user id
                var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                // Get refresh token from cookies
                var refreshToken = cookieService.GetRefreshToken();
                // Chcek refresh token
                if (refreshToken != null && refreshToken == await jwtDataService.GetRefreshTokenDataAsync(userIdClaim.Value))
                {
                    // Get new created access and refresh tokens
                    (accessToken, refreshToken) = jwtGenService.GenerateJwtTokens(claims.ToList());
                    // Set new tokens to cookies
                    cookieService.SetTokens(accessToken, refreshToken);
                    // Add new refresh token to storage
                    await jwtDataService.AddRefreshTokenAsync(refreshToken, new UserModel { Id = userIdClaim.Value });
                }
            }

            // Add access token to authorization header
            context.Request.Headers.Authorization = "Bearer " + accessToken;
            await next.Invoke(context);
        }
    }
}
