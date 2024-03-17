using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MainApp.Services
{
    internal class JwtService : IJwtService
    {
        private readonly ILogger<JwtService> logger;  
        private readonly UserContext dataContext;
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ICookieService cookieService;

        public JwtService(
            ILogger<JwtService> logger,
            UserContext dataContext, 
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ICookieService cookieService) 
        {
            this.logger = logger;
            this.dataContext = dataContext;
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
            this.cookieService = cookieService;
        }


        // Creation new tokens for user
        public async void GenerateTokens(UserModel user, List<Claim> authClaims)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateAccessToken(authClaims);     
            cookieService.SetTokens(accessToken, refreshToken);
            await AddRefreshTokenToDB(refreshToken, user);
        }


        // Acess token methods
        private string GenerateAccessToken(List<Claim> authClaims)
        {
            var jwtAccessToken = new JwtSecurityToken(
                issuer: configuration["JwtSettings:ISSUER"],
                audience: configuration["JwtSettings:AUDIENCE"],
                claims: authClaims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(5)),
                signingCredentials: new SigningCredentials(GetSecurityKey(), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtAccessToken);
        }
        private SymmetricSecurityKey GetSecurityKey()
        {
            return new SymmetricSecurityKey
            (
                Encoding.UTF8.GetBytes(configuration["JwtSettings:KEY"]!)
            );
        }
        public void CheckAccessToken(string accessToken)
        {

        }


        // Refresh token methods
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = new RNGCryptoServiceProvider())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        public void CheckRefreshToken(string refreshToken)
        {

        }
        public void RevokeRefreshToken()
        {

        }


        // Additional methods for tokens working
        private string GetUserIP()
        {
            return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        private async Task AddRefreshTokenToDB(string refreshToken, UserModel user)
        {
            var refreshTokenData = new RefreshTokenModel { Id = Guid.NewGuid().ToString(), User = user };
            refreshTokenData.TokensWhiteList.Add(GetUserIP(), refreshToken);
            await dataContext.RefreshTokens.AddAsync(refreshTokenData);
            dataContext.SaveChanges();
        }
        // Check count of user's refresh tokens (max 5) in DB
        public async void CheckUserRefreshTokens(UserModel user)
        {
            var userTokens = await dataContext.RefreshTokens.FirstOrDefaultAsync(x => x.User == user);
            if (userTokens.TokensWhiteList.Count == 5)
            {
                userTokens.TokensWhiteList.Clear();
                dataContext.Update(userTokens);
                await dataContext.SaveChangesAsync();
            }
        }
    }
}
