using MainApp.Services.Entry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace MainApp.Tests.Entry.CookieServiceTests
{
    public class SetCookieServiceTests : BaseCookieServiceTests
    {
        [Fact]
        public void SetCookie_SetAccessToken_Success()
        {
            // Arrange
            var accessKey = "accessKey";
            var accessValue = "accessValue";

            var method = typeof(CookieService).GetMethod("SetCookie", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { accessKey, accessValue, null };

            // Act
            method.Invoke(_cookieService, parameters);

            // Accept
            _mockResponseCookies
                .Verify(c => c.Append(accessKey, accessValue, It.Is<CookieOptions>(
                    co => co.HttpOnly)), Times.Once);
        }

        [Fact]
        public void SetCookie_SetRefreshToken_Success()
        {
            // Arrange
            var refreshKey = "refreshKey";
            var refreshValue = "refreshValue";
            var expires = DateTime.UtcNow.AddDays(30);

            var method = typeof(CookieService).GetMethod("SetCookie", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { refreshKey, refreshValue, expires };

            // Act
            method.Invoke(_cookieService, parameters);

            // Accept
            _mockResponseCookies
                .Verify(c => c.Append(refreshKey, refreshValue, It.Is<CookieOptions>(
                    co => co.HttpOnly)), Times.Once);
        }

        [Fact]
        public void SetCookie_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var testKey = "testKey";
            var testValue = "testValue";

            _mockResponseCookies
                .Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()))
                .Throws(new Exception("Test exception"));

            var method = typeof(CookieService).GetMethod("SetCookie", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { testKey, testValue, null };

            // Act
            method.Invoke(_cookieService, parameters);

            // Accept
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
