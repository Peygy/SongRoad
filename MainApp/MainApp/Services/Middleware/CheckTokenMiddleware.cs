using MainApp.Models.Service;
using MainApp.Models;
using System.Security.Claims;

namespace MainApp.Services
{
    // Middleware for check tokens
    public class CheckTokenMiddleware
    {
        private readonly RequestDelegate next;

        public CheckTokenMiddleware(RequestDelegate next)
        {
            this.next = next;
        }


        public async Task InvokeAsync(HttpContext context, ICookieService cookieService, IJwtGenService jwtGenService, IJwtDataService jwtDataService)
        {
            var accessToken = cookieService.GetAccessToken();
            if (accessToken != null && !jwtGenService.ValidAccessToken(accessToken))
            {
                var claims = jwtGenService.GetTokenUserClaims(accessToken).Claims;
                var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                var refreshToken = cookieService.GetRefreshToken();
                if (refreshToken != null && refreshToken == await jwtDataService.GetRefreshTokenDataAsync(userIdClaim.Value))
                {
                    (accessToken, refreshToken) = jwtGenService.GenerateJwtTokens(claims.ToList());
                    cookieService.SetTokens(accessToken, refreshToken);
                    await jwtDataService.AddRefreshTokenAsync(refreshToken, new UserModel { Id = userIdClaim.Value });
                }
            }

            context.Request.Headers.Authorization = "Bearer " + accessToken;
            await next.Invoke(context);
        }
    }
}
