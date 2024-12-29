using MainApp.Models.User;

namespace MainApp.Tests.Crew.UserManageServiceTests
{
    public class BanUserManageServiceTests : BaseUserManageServiceTests
    {
        public BanUserManageServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetUserBannedAsync_UserRightsIsNull_ReturnFalse()
        {
            // Arrange
            var userId = "userId";

            // Act
            var result = await _userManageService.GetUserWarnCountAsync(userId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetUserBannedAsync_UserRightsExists_ReturnBanState_True()
        {
            // Arrange
            var userRights = new UserRights()
            {
                User = new UserModel
                {
                    Id = Guid.NewGuid().ToString(),
                },
                Banned = true
            };
            _userContext.UserRights.Add(userRights);
            await _userContext.SaveChangesAsync();

            Assert.NotNull(userRights.UserId);

            // Act
            var result = await _userManageService.GetUserBannedAsync(userRights.UserId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetUserBannedAsync_UserRightsExists_ReturnBanState_False()
        {
            // Arrange
            var userRights = new UserRights()
            {
                User = new UserModel
                {
                    Id = Guid.NewGuid().ToString(),
                },
                Banned = false
            };
            _userContext.UserRights.Add(userRights);
            await _userContext.SaveChangesAsync();

            Assert.NotNull(userRights.UserId);

            // Act
            var result = await _userManageService.GetUserBannedAsync(userRights.UserId);

            // Assert
            Assert.False(result);
        }
    }
}
