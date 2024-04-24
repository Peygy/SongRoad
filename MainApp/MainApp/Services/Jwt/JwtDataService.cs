using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Services
{
    public class JwtDataService : IJwtDataService
    {
        private readonly ILogger<JwtDataService> log;
        private readonly UserContext dataContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        public JwtDataService(ILogger<JwtDataService> log, UserContext dataContext, IHttpContextAccessor httpContextAccessor)
        {
            this.log = log;
            this.dataContext = dataContext;
            this.httpContextAccessor = httpContextAccessor;
        }


        // Check refresh tokens count (max = 5)
        public async Task CheckUserRefreshTokensCountAsync(UserModel user)
        {
            try
            {
                var refreshToken = await dataContext.RefreshTokens.FirstOrDefaultAsync(u => u.User == user);
                var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                if (refreshToken != null && !refreshToken.TokensWhiteList.Keys.Contains(ip) && refreshToken.TokensWhiteList.Count == 5)
                {
                    refreshToken.TokensWhiteList.Clear();
                    await dataContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        // Add refresh token to database
        public async Task AddRefreshTokenAsync(string refreshToken, UserModel user)
        {
            try
            {
                var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id)
                    ?? new RefreshTokenModel { 
                        Id = Guid.NewGuid().ToString(),
                        UserId = user.Id
                    };

                var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                refreshTokenData.TokensWhiteList[ip] = refreshToken;
                await dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public async Task<string> GetRefreshTokenDataAsync(string userId)
        {
            var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            return refreshTokenData?.TokensWhiteList.GetValueOrDefault(ip);
        }
    }
}
