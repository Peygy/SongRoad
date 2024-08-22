using System.Security.Claims;

namespace MainApp.Tests.Entry.JwtGenServiceTests
{
    public class GetJwtGenServiceTests : BaseJwtGenServiceTests
    {
        [Fact]
        public void GetTokenUserClaims_ShouldReturnClaimsPrincipalForValidToken()
        {
            // Arrange
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            };
            var (accessToken, _) = _jwtGenService.GenerateJwtTokens(authClaims);

            // Act
            var claimsPrincipal = _jwtGenService.GetTokenUserClaims(accessToken);

            // Assert
            Assert.NotNull(claimsPrincipal);
            Assert.Contains(claimsPrincipal!.Claims, c => c.Type == ClaimTypes.Name && c.Value == "TestUser");
        }

        [Fact]
        public void GetTokenUserClaims_ShouldReturnNullForInvalidToken()
        {
            // Arrange
            var invalidToken = "invalid-token";

            // Act
            var claimsPrincipal = _jwtGenService.GetTokenUserClaims(invalidToken);

            // Assert
            Assert.Null(claimsPrincipal);
        }
    }
}
