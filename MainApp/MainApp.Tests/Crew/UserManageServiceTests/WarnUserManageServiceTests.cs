using MainApp.Models.User;

namespace MainApp.Tests.Crew.UserManageServiceTests
{
    public class WarnUserManageServiceTests : BaseUserManageServiceTests
    {
        public WarnUserManageServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetUserWarnCountAsync_UserRightsIsNull_ReturnZeroWarns()
        {
            // Arrange
            var userId = "userId";

            // Act
            var result = await _userManageService.GetUserWarnCountAsync(userId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetUserWarnCountAsync_UserRightsExists_ReturnWarnsCount()
        {
            // Arrange
            var warnsCount = 2;
            var userRights = new UserRights()
            {
                User = new UserModel
                {
                    Id = Guid.NewGuid().ToString(),
                },
                WarnCount = warnsCount
            };
            _userContext.UserRights.Add(userRights);
            await _userContext.SaveChangesAsync();

            Assert.NotNull(userRights.UserId);

            // Act
            var result = await _userManageService.GetUserWarnCountAsync(userRights.UserId);

            // Assert
            Assert.Equal(warnsCount, result);
        }

        [Fact]
        public async Task AddWarnToUserAsync_UserRightsIsNull_OneWarnAndNewUserRightsRecord()
        {
            // Arrange
            var userId = "userId";

            // Act
            var result = await _userManageService.AddWarnToUserAsync(userId);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task AddWarnToUserAsync_UserRightsExists_IncrementWarn_NoBan()
        {

        }

        [Fact]
        public async Task AddWarnToUserAsync_UserRightsExists_IncrementWarn_AddBanSuccess()
        {

        }

        [Fact]
        public async Task AddWarnToUserAsync_UserRightsExists_IncrementWarn_AddBanFailed()
        {

        }
    }
}
