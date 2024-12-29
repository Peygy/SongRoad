using MainApp.Models.User;
using Moq;

namespace MainApp.Tests.Crew.UserManageServiceTests
{
    public class GetUserManageServiceTests : BaseUserManageServiceTests
    {
        public GetUserManageServiceTests(UserContextWepAppFactory factory) : base(factory) { }

        [Fact]
        public async Task GetAllUsersAsync_ReturnUsersExcludingModeratorsAndAdmins()
        {
            // Arrange
            var users = new List<UserModel>
            {
                new UserModel { UserName = "User1" },
                new UserModel { UserName = "Moderator" },
                new UserModel { UserName = "User2" },
                new UserModel { UserName = "Admin" }
            };

            _mockUserManager.Setup(userManager => userManager.Users).Returns(users.AsQueryable());
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "User1")))
                .ReturnsAsync(new List<string> { UserRoles.User });
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "Moderator")))
                .ReturnsAsync(new List<string> { UserRoles.Moderator });
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "User2")))
                .ReturnsAsync(new List<string> { UserRoles.User });
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "Admin")))
                .ReturnsAsync(new List<string> { UserRoles.Admin });

            // Act
            var result = await _userManageService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.DoesNotContain(result, u => u.UserName == "Moderator");
            Assert.DoesNotContain(result, u => u.UserName == "Admin");
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenNoModeratorsOrAdminsExist_ReturnAllUsers()
        {
            // Arrange
            var users = new List<UserModel>
            {
                new UserModel { UserName = "User1" },
                new UserModel { UserName = "User2" },
                new UserModel { UserName = "User3" }
            };

            _mockUserManager.Setup(userManager => userManager.Users).Returns(users.AsQueryable());
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "User1")))
                .ReturnsAsync(new List<string> { UserRoles.User });
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "User2")))
                .ReturnsAsync(new List<string> { UserRoles.User });
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "User3")))
                .ReturnsAsync(new List<string> { UserRoles.User });

            // Act
            var result = await _userManageService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.DoesNotContain(result, u => u.UserName == "Moderator");
            Assert.DoesNotContain(result, u => u.UserName == "Admin");
        }

        [Fact]
        public async Task GetAllUsersAsync_AllUsersAreModeratorsOrAdmins_ReturnEmpty()
        {
            // Arrange
            var users = new List<UserModel>
            {
                new UserModel { UserName = "Moderator" },
                new UserModel { UserName = "Admin" }
            };

            _mockUserManager.Setup(userManager => userManager.Users).Returns(users.AsQueryable());
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "Moderator")))
                .ReturnsAsync(new List<string> { UserRoles.Moderator });
            _mockUserManager.Setup(userManager => userManager.GetRolesAsync(It.Is<UserModel>(u => u.UserName == "Admin")))
                .ReturnsAsync(new List<string> { UserRoles.Admin });

            // Act
            var result = await _userManageService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.DoesNotContain(result, u => u.UserName == "Moderator");
            Assert.DoesNotContain(result, u => u.UserName == "Admin");
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnEmptyList_ReturnEmpty()
        {
            // Arrange
            var users = new List<UserModel>();

            _mockUserManager.Setup(userManager => userManager.Users).Returns(users.AsQueryable());

            // Act
            var result = await _userManageService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
