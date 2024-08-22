using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace MainApp.Tests.Entry.RefershTokenServiceTests
{
    public class AddRefershTokenServiceTests : BaseRefershTokenServiceTests
    {
        public AddRefershTokenServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task AddRefreshTokenAsync_AddNewRefreshToken_RefreshTokenDataIsNull()
        {
            // Arrange
            var refreshToken = "refreshToken1";
            var userName = "userName1";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };

            // Act
            await _refershTokenService.AddRefreshTokenAsync(refreshToken, userModel);

            // Assert
            var refreshTokenData = await _userContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.User == userModel);
            Assert.NotNull(refreshTokenData);

            var ipAddress = (_mockHttpContextAccessor.Object).HttpContext.Connection.RemoteIpAddress.ToString();
            Assert.NotNull(ipAddress);
            Assert.NotEmpty(refreshTokenData.TokensWhiteList);
            Assert.Equal(refreshToken, refreshTokenData.TokensWhiteList[ipAddress]);
        }

        [Fact]
        public async Task AddRefreshTokenAsync_AddNewRefreshToken_RefreshTokenDataNotNull()
        {
            // Arrange
            var refreshToken = "refreshToken2";
            var userName = "userName2";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };

            var ipAddress = (_mockHttpContextAccessor.Object).HttpContext.Connection.RemoteIpAddress.ToString();
            Assert.NotNull(ipAddress);

            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel,
                TokensWhiteList = new Dictionary<string, string>
                {
                    { ipAddress, "token1" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            await _refershTokenService.AddRefreshTokenAsync(refreshToken, userModel);

            // Assert
            var refreshTokenData = await _userContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.User == userModel);
            Assert.NotNull(refreshTokenData);

            Assert.NotEmpty(refreshTokenData.TokensWhiteList);
            Assert.Equal(refreshToken, refreshTokenData.TokensWhiteList[ipAddress]);
        }

        [Fact]
        public async Task AddRefreshTokenAsync_AddNewRefreshToken_ThrownException()
        {
            // Arrange
            var refreshToken = "refreshToken3";
            var userName = "userName3";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };

            _mockConnection.Setup(c => c.RemoteIpAddress).Returns((IPAddress?)null);

            // Act
            await _refershTokenService.AddRefreshTokenAsync(refreshToken, userModel);

            // Accept
            _logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IP Address cannot be null or empty.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
