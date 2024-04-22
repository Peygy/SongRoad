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
                var userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                var refreshToken = cookieService.GetRefreshToken();
                if (refreshToken != null && refreshToken == await jwtDataService.GetRefreshTokenDataAsync(userId.Value))
                {
                    var tokens = jwtGenService.GenerateJwtTokens(claims.ToList());

                    cookieService.SetTokens(tokens.Item1, tokens.Item2);
                    await jwtDataService.AddRefreshTokenAsync(tokens.Item2, new UserModel { Id = userId.Value });
                    // Add new access token to headers
                    context.Request.Headers["Authorization"] = "Bearer " + tokens.Item1;
                }
            }

            await next.Invoke(context);
        }
    }
}
