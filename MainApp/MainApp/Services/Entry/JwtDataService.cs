using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using MainApp.Data;

namespace MainApp.Services.Entry
{
    public interface IJwtDataService
    {
        // Check user count of refresh tokens in db
        Task CheckUserRefreshTokensCountAsync(UserModel user);
        // Add new user refresh token to db
        Task AddRefreshTokenAsync(string refreshToken, UserModel user);
        // Get user refresh token from db
        Task<string> GetRefreshTokenDataAsync(string userId);
        // Remove refresh token from db
        Task RemoveRefreshTokenDataAsync(string userId);
    }

    /// <summary>
    /// Class of service for generate access and refresh tokens
    /// </summary>
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

        /// <summary>
        /// Method for check refresh tokens count (max = 5)
        /// </summary>
        /// <param name="user">Current user</param>
        /// <returns>Task object</returns>
        public async Task CheckUserRefreshTokensCountAsync(UserModel user)
        {
            try
            {
                // Get refresh token from storage
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

        /// <summary>
        /// Method for add refresh token session to storage
        /// </summary>
        /// <param name="refreshToken">Refresh token</param>
        /// <param name="user">Current user</param>
        /// <returns>Task object</returns>
        public async Task AddRefreshTokenAsync(string refreshToken, UserModel user)
        {
            try
            {
                // Get refresh token data from storage
                var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == user.Id);

                if (refreshTokenData == null)
                {
                    // Add refresh token to storage
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

        /// <summary>
        /// Method for get refresh token data by user IP
        /// </summary>
        /// <param name="userId">User identificator</param>
        /// <returns>Refresh token</returns>
        public async Task<string> GetRefreshTokenDataAsync(string userId)
        {
            var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            return refreshTokenData?.TokensWhiteList.GetValueOrDefault(GetUserRemoteIP());
        }

        /// <summary>
        /// Method for remove refresh token session from storage
        /// </summary>
        /// <param name="userId">User identificator</param>
        /// <returns>Task object</returns>
        public async Task RemoveRefreshTokenDataAsync(string userId)
        {
            var refreshTokenData = await dataContext.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            refreshTokenData.TokensWhiteList.Remove(GetUserRemoteIP());
            await dataContext.SaveChangesAsync();
        }


        /// <summary>
        /// Method for get user IP address
        /// </summary>
        /// <returns>IP address</returns>
        private string GetUserRemoteIP()
        {
            return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
