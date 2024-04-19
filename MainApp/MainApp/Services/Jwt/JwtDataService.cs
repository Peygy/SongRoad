using MainApp.Models;
using MainApp.Models.Service;

namespace MainApp.Services.Jwt
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

        // Add refresh token to database
        public async Task AddRefreshTokenAsync(string refreshToken, UserModel user)
        {
            try
            {
                var refreshTokenData = new RefreshTokenModel { Id = Guid.NewGuid().ToString(), User = user };

                var ip = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                refreshTokenData.TokensWhiteList.Add(ip, refreshToken);
                await dataContext.RefreshTokens.AddAsync(refreshTokenData);
                dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }
    }
}
