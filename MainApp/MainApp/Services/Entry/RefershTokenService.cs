using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using MainApp.Data;

namespace MainApp.Services.Entry
{
    /// <summary>
    /// Defines the contract for a service that interacts with refresh token.
    /// </summary>
    public interface IRefershTokenService
    {
        /// <summary>
        /// Checks count of <paramref name="user"/> refresh tokens (for multisession) 
        /// in <paramref name="user"/>'s refresh tokens dictionary (TokensWhiteList).
        /// </summary>
        /// 
        /// <remarks>
        ///     <para>
        ///     Also clear all <paramref name="user"/>'s refresh tokens, if all conditions are met:
        ///         <list type="bullet">
        ///             <item>
        ///                 <see cref="RefreshTokenModel"/> record exists in database;
        ///             </item>
        ///             <item>
        ///                 Current user's ip address not exists in <paramref name="user"/>'s 
        ///                 dictionary as a key of pair;
        ///             </item>
        ///             <item>
        ///                 Count of refresh tokens in <paramref name="user"/>'s dictionary equal 
        ///                 with 5 (max count of refresh tokens).
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// 
        /// <param name="user">Data of current user.</param>
        /// 
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task CheckUserRefreshTokensCountAsync(UserModel user);

        /// <summary>
        /// Adds new <paramref name="user"/> refresh token into database, if it not exixts, 
        /// or updates it, if it exists.
        /// </summary>
        /// <param name="refreshToken">New user's refresh token.</param>
        /// <param name="user">Data of current user.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task AddRefreshTokenAsync(string refreshToken, UserModel user);

        /// <summary>
        /// Gets from database refresh token of user, 
        /// who has specific identifier <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task<string> GetRefreshTokenDataAsync(string userId);

        /// <summary>
        /// Removes from database refresh token of user, 
        /// who has specific identifier <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task RemoveRefreshTokenDataAsync(string userId);
    }

    public class RefershTokenService : IRefershTokenService
    {
        /// <summary>
        /// The logger instance used 
        /// for logging messages related to the <see cref="RefershTokenService"/>.
        /// </summary>
        private readonly ILogger<RefershTokenService> log;
        /// <summary>
        /// The database context used for accessing the users data.
        /// </summary>
        private readonly UserContext userContext;
        /// <summary>
        /// Provides access to the current HTTP context, including request and response details.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefershTokenService"/> class.
        /// </summary>
        /// <param name="log">
        /// The logger instance used 
        /// for logging messages related to the <see cref="RefershTokenService"/>.
        /// </param>
        /// <param name="userContext">
        /// The database context used for accessing the users data.
        /// </param>
        /// <param name="httpContextAccessor">
        /// Provides access to the current HTTP context, including request and response details.
        /// </param>
        public RefershTokenService(
            ILogger<RefershTokenService> log, 
            UserContext userContext, 
            IHttpContextAccessor httpContextAccessor)
        {
            this.log = log;
            this.userContext = userContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task CheckUserRefreshTokensCountAsync(UserModel user)
        {
            try
            {
                // Get refresh token from storage
                var userRefreshTokensRecord = await userContext.RefreshTokens
                    .FirstOrDefaultAsync(u => u.User == user);

                if (userRefreshTokensRecord != null && 
                    !userRefreshTokensRecord.TokensWhiteList.Keys.Contains(GetUserRemoteIPAddress()) && 
                    userRefreshTokensRecord.TokensWhiteList.Count == 5)
                {
                    userRefreshTokensRecord.TokensWhiteList.Clear();
                    await userContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public async Task AddRefreshTokenAsync(string refreshToken, UserModel user)
        {
            try
            {
                // Get refresh token data from storage
                var refreshTokenData = await userContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.User == user);

                if (refreshTokenData == null)
                {
                    // Add refresh token to storage
                    refreshTokenData = new RefreshTokenModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        User = user
                    };

                    refreshTokenData.TokensWhiteList[GetUserRemoteIPAddress()] = refreshToken;
                    userContext.RefreshTokens.Add(refreshTokenData);
                }
                else
                {
                    refreshTokenData.TokensWhiteList[GetUserRemoteIPAddress()] = refreshToken;
                    userContext.Update(refreshTokenData);
                }

                await userContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
            }
        }

        public async Task<string> GetRefreshTokenDataAsync(string userId)
        {
            var refreshTokenData = await userContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);
            return refreshTokenData?.TokensWhiteList.GetValueOrDefault(GetUserRemoteIPAddress());
        }

        public async Task RemoveRefreshTokenDataAsync(string userId)
        {
            var refreshTokenData = await userContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);
            refreshTokenData.TokensWhiteList.Remove(GetUserRemoteIPAddress());
            await userContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets user's ip address for its further use in the formation 
        /// of an address and refresh token pair.
        /// </summary>
        /// <returns>
        /// User's ip address.
        /// </returns>
        private string GetUserRemoteIPAddress()
        {
            return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
