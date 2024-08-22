using MainApp.Models.User;
using MainApp.Services.Entry;
using Moq;
using System.Security.Claims;

namespace MainApp.Tests.Entry.AuthServiceTests
{
    public class LogoutAuthServiceTests : BaseAuthServiceTests
    {
        [Fact]
        public async Task Logout_NoAccessToken_ReturnFalse()
        {
            // Arrange
            _mockCookieService
                .Setup(userManager => userManager.GetAccessToken())
                .Returns(It.IsAny<string>());

            // Act
            var result = await _authService.Logout();

            // Accept
            Assert.False(result);

            _mockCookieService
                .Verify(userManager => userManager.GetAccessToken(), Times.Once);
            _mockJwtGenService
                .Verify(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Logout_ClaimsIsNull_ReturnFalse()
        {
            // Arrange
            var accessToken = "accessToken";

            _mockCookieService
                .Setup(userManager => userManager.GetAccessToken())
                .Returns(accessToken);
            _mockJwtGenService
                .Setup(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()))
                .Returns((ClaimsPrincipal?)null);

            // Act
            var result = await _authService.Logout();

            // Accept
            Assert.False(result);

            _mockCookieService
                .Verify(userManager => userManager.GetAccessToken(), Times.Once);
            _mockJwtGenService
                .Verify(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Logout_UserIdClaimIsNull_ReturnFalse()
        {
            // Arrange
            var accessToken = "accessToken";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            _mockCookieService
                .Setup(userManager => userManager.GetAccessToken())
                .Returns(accessToken);
            _mockJwtGenService
                .Setup(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()))
                .Returns(claimsPrincipal);

            // Act
            var result = await _authService.Logout();

            // Accept
            Assert.False(result);

            _mockCookieService
                .Verify(userManager => userManager.GetAccessToken(), Times.Once);
            _mockJwtGenService
                .Verify(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()), Times.Once);
            _mockCookieService
                .Verify(cookieService => cookieService.DeleteTokens(), Times.Never);
            _mockJwtDataService
                .Verify(jwtDataService => jwtDataService.RemoveRefreshTokenDataAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Logout_Success_ReturnTrue()
        {
            // Arrange
            var accessToken = "accessToken";
            var userId = "12345";
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _mockCookieService
                .Setup(userManager => userManager.GetAccessToken())
                .Returns(accessToken);
            _mockJwtGenService
                .Setup(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()))
                .Returns(claimsPrincipal);
            _mockCookieService
                .Setup(cookieService => cookieService.DeleteTokens());
            _mockJwtDataService
                .Setup(jwtDataService => jwtDataService.RemoveRefreshTokenDataAsync(It.IsAny<string>()));

            // Act
            var result = await _authService.Logout();

            // Accept
            Assert.True(result);

            _mockCookieService
                .Verify(userManager => userManager.GetAccessToken(), Times.Once);
            _mockJwtGenService
                .Verify(jwtGenService => jwtGenService.GetTokenUserClaims(It.IsAny<string>()), Times.Once);
            _mockCookieService
                .Verify(cookieService => cookieService.DeleteTokens(), Times.Once);
            _mockJwtDataService
                .Verify(jwtDataService => jwtDataService.RemoveRefreshTokenDataAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
