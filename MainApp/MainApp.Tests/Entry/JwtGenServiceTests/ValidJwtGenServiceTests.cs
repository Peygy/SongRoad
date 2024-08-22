using System.Security.Claims;

namespace MainApp.Tests.Entry.JwtGenServiceTests
{
    public class ValidJwtGenServiceTests : BaseJwtGenServiceTests
    {
        [Fact]
        public void ValidAccessToken_ShouldReturnTrueForValidToken()
        {
            // Arrange
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            };
            var (accessToken, _) = _jwtGenService.GenerateJwtTokens(authClaims);

            // Act
            var result = _jwtGenService.ValidAccessToken(accessToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidAccessToken_ShouldReturnFalseForInvalidToken()
        {
            // Arrange
            var invalidToken = "invalid-token";

            // Act
            var result = _jwtGenService.ValidAccessToken(invalidToken);

            // Assert
            Assert.False(result);
        }
    }
}
