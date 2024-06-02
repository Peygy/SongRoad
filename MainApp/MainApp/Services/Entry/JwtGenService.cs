using MainApp.Interfaces.Entry;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MainApp.Services
{
    /// <summary>
    /// Class of service for generate access and refresh tokens
    /// </summary>
    public class JwtGenService : IJwtGenService
    {
        private readonly IConfiguration configuration;

        public JwtGenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        /// <summary>
        /// Method for create new tokens for user
        /// </summary>
        /// <param name="authClaims">User data for create tokens</param>
        /// <returns>Tuple of access and refresh tokens</returns>
        public (string, string) GenerateJwtTokens(List<Claim> authClaims)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateAccessToken(authClaims);
            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Method for create access token
        /// </summary>
        /// <param name="authClaims">User data for create token</param>
        /// <returns>Access token</returns>
        private string GenerateAccessToken(List<Claim> authClaims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:KEY"]!));

            var jwtAccessToken = new JwtSecurityToken(
                issuer: configuration["JwtSettings:ISSUER"],
                audience: configuration["JwtSettings:AUDIENCE"],
                claims: authClaims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
                signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtAccessToken);
        }

        /// <summary>
        /// Method for create refresh token
        /// </summary>
        /// <returns>Refresh token</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        /// <summary>
        /// Method for validate access token
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <returns>State of validate access token</returns>
        public bool ValidAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:KEY"]!)),
                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:ISSUER"],
                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:AUDIENCE"],
                ValidateLifetime = true
            };

            try
            {
                tokenHandler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);
                if (validatedToken.ValidTo <= DateTime.UtcNow)
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method for get user data from access token
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <returns>User data from access token</returns>
        /// <exception cref="ArgumentException">Exception for invalid JWT token</exception>
        public ClaimsPrincipal GetTokenUserClaims(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            if (jwtToken == null)
            {
                throw new ArgumentException("Invalid JWT token.");
            }

            var claims = new ClaimsIdentity(jwtToken.Claims.Where(c => c.Type.Contains("http")));

            return new ClaimsPrincipal(claims);
        }
    }
}
