using MainApp.Models.User;
using MainApp.Services.Entry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Reflection;

namespace MainApp.Tests.Entry.RefershTokenServiceTests
{
    public class GetRefershTokenServiceTests : BaseRefershTokenServiceTests
    {
        public GetRefershTokenServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetRefreshTokenDataAsync_ReturnRefreshToken()
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
            var result = await _refershTokenService.GetRefreshTokenDataAsync(userModel.Id);

            // Accept
            Assert.NotNull(result);
            Assert.Equal("token1", result);
        }

        [Fact]
        public async Task GetRefreshTokenDataAsync_ReturnDefaultForString()
        {
            // Arrange
            var userName = "userName2";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };
            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            var result = await _refershTokenService.GetRefreshTokenDataAsync(userModel.Id);

            // Accept
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRefreshTokenDataAsync_NotRefreshToken_ReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            // Act
            var result = await _refershTokenService.GetRefreshTokenDataAsync(userId);

            // Accept
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRefreshTokenDataAsync_ThrownException_NoIpAddress()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();

            _mockConnection.Setup(c => c.RemoteIpAddress).Returns((IPAddress?)null);

            // Act
            await _refershTokenService.GetRefreshTokenDataAsync(userId);

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

        [Fact]
        public async Task GetUserRemoteIP_Success_ReturnCurrentIPAddress()
        {
            // Arrange
            var method = typeof(RefershTokenService).GetMethod("GetUserRemoteIPAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = [];

            // Act
            var result = method.Invoke(_refershTokenService, parameters);

            // Accept
            Assert.NotNull(result);
            Assert.Equal("127.0.0.1", result);
        }

        [Fact]
        public async Task GetUserRemoteIP_ReturnStringEmpty_HttpContextIsNull()
        {
            // Arrange
            var method = typeof(RefershTokenService).GetMethod("GetUserRemoteIPAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = [];
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            // Act
            var result = method.Invoke(_refershTokenService, parameters);

            // Accept
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public async Task GetUserRemoteIP_ReturnStringEmpty_IpAddressIsNull()
        {
            // Arrange
            var method = typeof(RefershTokenService).GetMethod("GetUserRemoteIPAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = [];
            _mockConnection.Setup(c => c.RemoteIpAddress).Returns((IPAddress?)null);

            // Act
            var result = method.Invoke(_refershTokenService, parameters);

            // Accept
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result);
        }
    }
}
