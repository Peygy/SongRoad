using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;

namespace MainApp.Tests.Entry.RefershTokenServiceTests
{
    public class CheckRefershTokenServiceTests : BaseRefershTokenServiceTests
    {
        public CheckRefershTokenServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task CheckUserRefreshTokensCountAsync_NotClearDictionary_NoRecord()
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
                    { "192.168.1.1", "token1" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            var newUserModel = new UserModel() { Id = Guid.NewGuid().ToString() };

            // Act
            await _refershTokenService.CheckUserRefreshTokensCountAsync(newUserModel);

            // Assert
            Assert.NotEmpty(_userContext.RefreshTokens);

            var dbUser = await _userContext.RefreshTokens.FirstOrDefaultAsync(r => r.User == userModel);
            Assert.NotNull(dbUser);
            Assert.NotNull(dbUser.User);
            Assert.Equal(userName, dbUser.User.UserName);
            Assert.NotEmpty(dbUser.TokensWhiteList);
        }

        [Fact]
        public async Task CheckUserRefreshTokensCountAsync_NotClearDictionary_AnyIpAddressInDictionary()
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
                    { "192.168.1.1", "token1" },
                    { "192.168.1.2", "token2" },
                    { "127.0.0.1", "token3" },
                    { "192.168.1.4", "token4" },
                    { "192.168.1.5", "token5" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            await _refershTokenService.CheckUserRefreshTokensCountAsync(userModel);

            // Assert
            Assert.NotEmpty(_userContext.RefreshTokens);

            var dbUser = await _userContext.RefreshTokens.FirstOrDefaultAsync(r => r.User == userModel);
            Assert.NotNull(dbUser);
            Assert.NotNull(dbUser.User);
            Assert.Equal(userName, dbUser.User.UserName);
            Assert.NotEmpty(dbUser.TokensWhiteList);
        }

        [Fact]
        public async Task CheckUserRefreshTokensCountAsync_NotClearDictionary_NotFiveItemsInDictionary()
        {
            // Arrange
            var userName = "userName3";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };
            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel,
                TokensWhiteList = new Dictionary<string, string>
                {
                    { "192.168.1.1", "token1" },
                    { "192.168.1.2", "token2" },
                    { "192.168.1.3", "token3" },
                    { "192.168.1.4", "token4" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            await _refershTokenService.CheckUserRefreshTokensCountAsync(userModel);

            // Assert
            Assert.NotEmpty(_userContext.RefreshTokens);

            var dbUser = await _userContext.RefreshTokens.FirstOrDefaultAsync(r => r.User == userModel);
            Assert.NotNull(dbUser);
            Assert.NotNull(dbUser.User);
            Assert.Equal(userName, dbUser.User.UserName);
            Assert.NotEmpty(dbUser.TokensWhiteList);
        }

        [Fact]
        public async Task CheckUserRefreshTokensCountAsync_ClearDictionary()
        {
            // Arrange
            var userName = "userName4";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };
            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel,
                TokensWhiteList = new Dictionary<string, string>
                {
                    { "192.168.1.1", "token1" },
                    { "192.168.1.2", "token2" },
                    { "192.168.1.3", "token3" },
                    { "192.168.1.4", "token4" },
                    { "192.168.1.5", "token5" }
                }
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            // Act
            await _refershTokenService.CheckUserRefreshTokensCountAsync(userModel);

            // Assert
            Assert.NotEmpty(_userContext.RefreshTokens);

            var dbUser = await _userContext.RefreshTokens.FirstOrDefaultAsync(r => r.User == userModel);
            Assert.NotNull(dbUser);
            Assert.NotNull(dbUser.User);
            Assert.Equal(userName, dbUser.User.UserName);
            Assert.Empty(dbUser.TokensWhiteList);
        }

        [Fact]
        public async Task CheckUserRefreshTokensCountAsync_LogsErrorWhenRemoteIpAddressIsNull()
        {
            // Arrange
            var userName = "userName5";
            var userModel = new UserModel() { Id = Guid.NewGuid().ToString(), UserName = userName };
            var record = new RefreshTokenModel()
            {
                Id = Guid.NewGuid().ToString(),
                User = userModel
            };
            _userContext.RefreshTokens.Add(record);
            await _userContext.SaveChangesAsync();

            _mockConnection.Setup(c => c.RemoteIpAddress).Returns((IPAddress?)null);

            // Act
            await _refershTokenService.CheckUserRefreshTokensCountAsync(userModel);

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
