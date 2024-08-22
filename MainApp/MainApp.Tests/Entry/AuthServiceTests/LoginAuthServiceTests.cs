using MainApp.DTO.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace MainApp.Tests.Entry.AuthServiceTests
{
    public class LoginAuthServiceTests : BaseAuthServiceTests
    {
        [Fact]
        public async Task UserLogin_NoUserWithInputUsername_ReturnFalse()
        {
            // Arrange
            var registerModel = new LoginModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };
            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<UserModel>());

            // Act
            var result = await _authService.UserLogin(registerModel);

            // Accept
            Assert.False(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UserLogin_UserPasswordIsNotValid_ReturnFalse()
        {
            // Arrange
            var registerModel = new LoginModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };
            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserModel());
            _mockUserManager
                .Setup(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.UserLogin(registerModel);

            // Accept
            Assert.False(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.GetRolesAsync(It.IsAny<UserModel>()), Times.Never);
        }

        [Fact]
        public async Task UserLogin_UserRolesCountIsZero_ReturnFalse()
        {
            // Arrange
            var registerModel = new LoginModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };
            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserModel());
            _mockUserManager
                .Setup(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockUserManager
                .Setup(userManager => userManager.GetRolesAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _authService.UserLogin(registerModel);

            // Accept
            Assert.False(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.GetRolesAsync(It.IsAny<UserModel>()), Times.Once);
        }

        [Fact]
        public async Task UserLogin_UserLoginSuccess_ReturnTrue()
        {
            // Arrange
            var registerModel = new LoginModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };
            var userModel = new UserModel()
            {
                UserName = "Test"
            };

            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(userModel);
            _mockUserManager
                .Setup(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockUserManager
                .Setup(userManager => userManager.GetRolesAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(new List<string>() { "Role1" });

            _mockJwtDataService
                .Setup(userManager => userManager.CheckUserRefreshTokensCountAsync(It.IsAny<UserModel>()));
            _mockJwtGenService
                .Setup(jwtGenService => jwtGenService.GenerateJwtTokens(It.IsAny<List<Claim>>()))
                .Returns(It.IsAny<(string, string)>);
            _mockCookieService
                .Setup(cookieService => cookieService.SetTokens(It.IsAny<string>(), It.IsAny<string>()));
            _mockJwtDataService
                .Setup(jwtDataService => jwtDataService.AddRefreshTokenAsync(It.IsAny<string>(), It.IsAny<UserModel>()));

            // Act
            var result = await _authService.UserLogin(registerModel);

            // Accept
            Assert.True(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CheckPasswordAsync(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.GetRolesAsync(It.IsAny<UserModel>()), Times.Exactly(2));

            _mockJwtDataService
                .Verify(userManager => userManager.CheckUserRefreshTokensCountAsync(It.IsAny<UserModel>()), Times.Once);
            _mockJwtGenService
                .Verify(jwtGenService => jwtGenService.GenerateJwtTokens(It.IsAny<List<Claim>>()), Times.Once);
            _mockCookieService
                .Verify(cookieService => cookieService.SetTokens(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockJwtDataService
                .Verify(jwtDataService => jwtDataService.AddRefreshTokenAsync(It.IsAny<string>(), It.IsAny<UserModel>()), Times.Once);
        }
    }
}
