using Microsoft.AspNetCore.Http;
using Moq;

namespace MainApp.Tests.Entry.CookieServiceTests
{
    public class DeleteCookieServiceTests : BaseCookieServiceTests
    {
        [Fact]
        public void DeleteTokens_ShouldDeleteTokens_WhenCookiesExist()
        {
            // Arrange
            _mockRequestCookies.Setup(c => c.ContainsKey("access_token")).Returns(true);

            // Act
            _cookieService.DeleteTokens();

            // Assert
            _mockResponseCookies
                .Verify(c => c.Append("access_token", "", It.Is<CookieOptions>(
                    co => co.Expires <= DateTime.Now)), Times.Once);
            _mockResponseCookies
                .Verify(c => c.Append("refresh_token", "", It.Is<CookieOptions>(
                    co => co.Expires <= DateTime.Now)), Times.Once);
        }

        [Fact]
        public void DeleteTokens_ShouldNotDeleteTokens_WhenCookiesDoNotExist()
        {
            // Arrange
            _mockRequestCookies.Setup(c => c.ContainsKey("access_token")).Returns(false);

            // Act
            _cookieService.DeleteTokens();

            // Assert
            _mockResponseCookies
                .Verify(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<CookieOptions>()), Times.Never);
        }
    }
}
