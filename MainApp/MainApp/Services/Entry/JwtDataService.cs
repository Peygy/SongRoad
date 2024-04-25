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

                if (refreshToken != null && 
                    !refreshToken.TokensWhiteList.Keys.Contains(GetUserRemoteIP()) && 
                    refreshToken.TokensWhiteList.Count == 5)
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
                var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);
                if (refreshTokenData == null)
                {
                    refreshTokenData = new RefreshTokenModel { Id = Guid.NewGuid().ToString(), UserId = user.Id };
                    refreshTokenData.TokensWhiteList[GetUserRemoteIP()] = refreshToken;
                    await dataContext.RefreshTokens.AddAsync(refreshTokenData);
                }
                else
                {
                    refreshTokenData.TokensWhiteList[GetUserRemoteIP()] = refreshToken;
                    dataContext.Update(refreshTokenData);
                }

                await dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public async Task<string> GetRefreshTokenDataAsync(string userId)
        {
            var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            return refreshTokenData?.TokensWhiteList.GetValueOrDefault(GetUserRemoteIP());
        }

        // Remove refresh token session from database
        public async Task RemoveRefreshTokenDataAsync(string userId)
        {
            var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            refreshTokenData.TokensWhiteList.Remove(GetUserRemoteIP());
            await dataContext.SaveChangesAsync();
        }

        // Get user IP-address
        private string GetUserRemoteIP()
        {
            return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
