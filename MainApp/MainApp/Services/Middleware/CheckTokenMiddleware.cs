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
            // Мб из контекста брать
            var accessToken = cookieService.GetAccessToken();
            if (accessToken != null && !jwtGenService.ValidAccessToken(accessToken))
            {
                var claims = jwtGenService.GetTokenClaims(accessToken);
                var userId = claims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                var refreshToken = cookieService.GetRefreshToken();
                if (refreshToken != null && refreshToken == await jwtDataService.GetRefreshTokenAsync(userId.Value))
                {
                    var tokens = jwtGenService.GenerateJwtTokens(claims.Claims.ToList());
                    await jwtDataService.AddTokensToStoragesAsync(tokens, new UserModel { Id = userId.Value });
                }
            }

            await next.Invoke(context);
        }
    }
}
