using MainApp.Models;
using Microsoft.IdentityModel.Tokens;
using System.Buffers.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MainApp.Services.Jwt
{
    internal class JwtGenService : IJwtGenService
    {
        private readonly IConfiguration configuration;

        public JwtGenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }


        // Creation of new tokens for user
        public (string, string) GenerateJwtTokens(List<Claim> authClaims)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateAccessToken(authClaims);
            return (accessToken, refreshToken);
        }


        // Create access token
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


        // Create refresh token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        // Check access token validation
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
                var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);

                if (validatedToken.ValidTo <= DateTime.UtcNow)
                {
                    return false;
                }

                // Ckeck less that 1 min
                if ((validatedToken.ValidTo - DateTime.UtcNow).TotalMinutes <= 1)
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

        public ClaimsPrincipal GetTokenUserClaims(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            if (jwtToken == null)
            {
                throw new ArgumentException("Invalid JWT token.");
            }

            var claims = new ClaimsIdentity(jwtToken.Claims.Take(3));

            return new ClaimsPrincipal(claims);
        }
    }
}
