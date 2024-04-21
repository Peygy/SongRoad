using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Services.Jwt
{
    public class JwtDataService : IJwtDataService
    {
        private readonly ILogger<JwtDataService> log;
        private readonly UserContext dataContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICookieService cookieService;

        public JwtDataService(ILogger<JwtDataService> log, UserContext dataContext, IHttpContextAccessor httpContextAccessor, ICookieService cookieService)
        {
            this.log = log;
            this.dataContext = dataContext;
            this.httpContextAccessor = httpContextAccessor;
            this.cookieService = cookieService;
        }


        // Check refresh tokens count (max = 5)
        public async Task CheckUserRefreshTokensCountAsync(UserModel user)
        {
            try
            {
                var refreshToken = dataContext.RefreshTokens.FirstOrDefault(u => u.User == user);
                var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                if (!refreshToken.TokensWhiteList.Keys.Contains(ip) && refreshToken.TokensWhiteList.Count == 5)
                {
                    refreshToken.TokensWhiteList.Clear();
                    dataContext.Update(refreshToken);
                    await dataContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public async Task AddTokensToStoragesAsync((string, string) tokens, UserModel user)
        {
            // Params: access token, refresh token
            cookieService.SetTokens(tokens.Item1, tokens.Item2);
            // Add refresh token to database
            await AddRefreshTokenAsync(tokens.Item2, user);
        }

        // Add refresh token to database
        private async Task AddRefreshTokenAsync(string refreshToken, UserModel user)
        {
            try
            {
                var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
                if (refreshTokenData == null)
                {
                    refreshTokenData = new RefreshTokenModel { Id = Guid.NewGuid().ToString(), UserId = user.Id };
                }

                var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                if (!refreshTokenData.TokensWhiteList.TryAdd(ip, refreshToken))
                {
                    refreshTokenData.TokensWhiteList[ip] = refreshToken;
                    dataContext.Update(refreshTokenData);
                }
                else
                {
                    await dataContext.RefreshTokens.AddAsync(refreshTokenData);
                }

                dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public async Task<string> GetRefreshTokenAsync(string userId)
        {
            var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            return refreshTokenData.TokensWhiteList.FirstOrDefault(t => t.Key == ip).Value;
        }
    }
}
