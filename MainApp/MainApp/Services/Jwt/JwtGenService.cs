using MainApp.Models;
using Microsoft.IdentityModel.Tokens;
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
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(5)),
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
    }
}
