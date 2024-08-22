using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace MainApp.Tests.Entry.RefershTokenServiceTests
{
    public class RemoveRefershTokenServiceTests : BaseRefershTokenServiceTests
    {
        public RemoveRefershTokenServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task RemoveRefreshTokenDataAsync_Success_RemoveRefershTokenData()
        {
            // Arrange
            var userName = "userName1";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };
            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel,
                TokensWhiteList = new Dictionary<string, string>
                {
                    { "127.0.0.1", "token1" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            await _refershTokenService.RemoveRefreshTokenDataAsync(userModel.Id);

            // Accept
            var refreshTokenData = await _userContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.User == userModel);
            Assert.NotNull(refreshTokenData);
            Assert.Empty(refreshTokenData.TokensWhiteList);
        }

        [Fact]
        public async Task RemoveRefreshTokenDataAsync_NotRemove_NoRecordExist()
        {
            // Arrange
            var userName = "userName2";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };
            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel,
                TokensWhiteList = new Dictionary<string, string>
                {
                    { "192.168.1.1", "token1" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            await _refershTokenService.RemoveRefreshTokenDataAsync(userModel.Id);

            // Accept
            var refreshTokenData = await _userContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.User == userModel);
            Assert.NotNull(refreshTokenData);
            Assert.NotEmpty(refreshTokenData.TokensWhiteList);
            Assert.DoesNotContain("127.0.0.1", refreshTokenData.TokensWhiteList.Keys);
        }

        [Fact]
        public async Task RemoveRefreshTokenDataAsync_ThrownException_NoIpAddress()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _mockConnection.Setup(c => c.RemoteIpAddress).Returns((IPAddress?)null);

            // Act
            await _refershTokenService.RemoveRefreshTokenDataAsync(userId);

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
