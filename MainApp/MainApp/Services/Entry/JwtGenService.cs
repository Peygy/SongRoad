using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MainApp.Services.Entry
{
    /// <summary>
    /// Defines the contract for a service that generates, validates JWT tokens, 
    /// and retrieves user claims from a JWT token.
    /// </summary>
    public interface IJwtGenService
    {
        /// <summary>
        /// Generates a JWT access token and a refresh token using the provided user <paramref name="authClaims"/>.
        /// </summary>
        /// <param name="authClaims">A list of user claims to be included in the JWT token.</param>
        /// <returns>
        /// A tuple containing the generated access token and refresh token.
        /// </returns>
        (string, string) GenerateJwtTokens(List<Claim> authClaims);

        /// <summary>
        /// Validates the provided JWT <paramref name="accessToken"/>.
        /// </summary>
        /// <param name="accessToken">The JWT access token to validate.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="accessToken"/> is valid; otherwise, <c>false</c>.
        /// </returns>
        bool ValidAccessToken(string accessToken);

        /// <summary>
        /// Retrieves the claims of the user from the provided JWT <paramref name="accessToken"/>.
        /// </summary>
        /// <param name="accessToken">The JWT access token from which to extract claims.</param>
        /// <returns>
        /// A <see cref="ClaimsPrincipal"/> containing the user's claims if the <paramref name="accessToken"/> is valid; 
        /// otherwise, <c>null</c>.
        /// </returns>
        ClaimsPrincipal? GetTokenUserClaims(string accessToken);
    }

    public class JwtGenService : IJwtGenService
    {
        /// <summary>
        /// Provides access to the application's configuration settings.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtGenService"/> class.
        /// </summary>
        /// <param name="configuration">Provides access to the application's configuration settings.</param>
        public JwtGenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public (string, string) GenerateJwtTokens(List<Claim> authClaims)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateAccessToken(authClaims);
            return (accessToken, refreshToken);
        }

        /// <summary>
        /// Generates a JWT access token using the provided user claims.
        /// </summary>
        /// <param name="authClaims">The claims to be included in the JWT token.</param>
        /// <returns>The generated JWT access token as a string.</returns>
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
        /// Generates a new refresh token, which is a random string used to obtain new access tokens.
        /// </summary>
        /// <returns>A base64 encoded string representing the generated refresh token.</returns>
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

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

        public ClaimsPrincipal? GetTokenUserClaims(string accessToken)
        {
            try
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
            catch (Exception)
            {
                return null;
            }
        }
    }
}
