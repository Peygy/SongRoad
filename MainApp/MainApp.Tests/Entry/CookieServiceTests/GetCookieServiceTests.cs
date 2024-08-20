namespace MainApp.Tests.Entry.CookieServiceTests
{
    public class GetCookieServiceTests : BaseCookieServiceTests
    {
        [Fact]
        public void GetAccessToken_ShouldReturnAccessToken_WhenCookieExists()
        {
            // Arrange
            string expectedToken = "accessToken";
            _mockRequestCookies.Setup(c => c["access_token"]).Returns(expectedToken);

            // Act
            var result = _cookieService.GetAccessToken();

            // Assert
            Assert.Equal(expectedToken, result);
        }

        [Fact]
        public void GetAccessToken_ShouldReturnNull_WhenCookieDoesNotExist()
        {
            // Arrange
            _mockRequestCookies.Setup(c => c["access_token"]).Returns((string?)null);

            // Act
            var result = _cookieService.GetAccessToken();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetRefreshToken_ShouldReturnRefreshToken_WhenCookieExists()
        {
            // Arrange
            string expectedToken = "refresh";
            _mockRequestCookies.Setup(c => c["refresh_token"]).Returns(expectedToken);

            // Act
            var result = _cookieService.GetRefreshToken();

            // Assert
            Assert.Equal(expectedToken, result);
        }

        [Fact]
        public void GetRefreshToken_ShouldReturnNull_WhenCookieDoesNotExist()
        {
            // Arrange
            _mockRequestCookies.Setup(c => c["refresh_token"]).Returns((string?)null);

            // Act
            var result = _cookieService.GetRefreshToken();

            // Assert
            Assert.Null(result);
        }
    }
}
