using MainApp.Services.Entry;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;

namespace MainApp.Tests.Entry.JwtGenServiceTests
{
    public class GenerateJwtGenServiceTests : BaseJwtGenServiceTests
    {
        [Fact]
        public void GenerateJwtTokens_ShouldReturnValidTokens()
        {
            // Arrange
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            };

            // Act
            var (accessToken, refreshToken) = _jwtGenService.GenerateJwtTokens(authClaims);

            // Assert
            Assert.False(string.IsNullOrEmpty(accessToken));
            Assert.False(string.IsNullOrEmpty(refreshToken));
        }

        [Fact]
        public void GenerateAccessToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            };

            var method = typeof(JwtGenService).GetMethod("GenerateAccessToken", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { authClaims };

            // Act
            string accessToken = (string)method.Invoke(_jwtGenService, parameters);

            // Assert
            Assert.False(string.IsNullOrEmpty(accessToken));

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(accessToken);

            Assert.Equal("TestIssuer", jwtToken.Issuer);
            Assert.Equal("TestAudience", jwtToken.Audiences.First());
            Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Name && c.Value == "TestUser");
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnNonEmptyString()
        {
            // Arrange
            var method = typeof(JwtGenService).GetMethod("GenerateRefreshToken", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = [];

            // Act
            string refreshToken = (string)method.Invoke(_jwtGenService, parameters);

            // Assert
            Assert.False(string.IsNullOrEmpty(refreshToken));

            var decodedBytes = Convert.FromBase64String(refreshToken);
            Assert.Equal(32, decodedBytes.Length);
        }
    }
}
