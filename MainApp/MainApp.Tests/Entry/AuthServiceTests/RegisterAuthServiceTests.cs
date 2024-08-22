using MainApp.DTO.User;
using MainApp.Models.User;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace MainApp.Tests.Entry.AuthServiceTests
{
    public class RegisterAuthServiceTests : BaseAuthServiceTests
    {
        [Fact]
        public async Task UserRegister_NoUserWithInputUsername_ReturnFalse()
        {
            // Arrange
            var registerModel = new RegisterModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };
            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new UserModel());

            // Act
            var result = await _authService.UserRegister(registerModel);

            // Accept
            Assert.False(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CreateAsync(It.IsAny<UserModel>(), registerModel.Password), Times.Never);
        }

        [Fact]
        public async Task UserRegister_NoSuccessCreationResult_ReturnFalse()
        {
            // Arrange
            var registerModel = new RegisterModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };
            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<UserModel>);

            var identityErrors = new IdentityError[]
            {
                new IdentityError()
                {
                    Code = "Username already exists",
                    Description = "Username already exists"
                }
            };
            _mockUserManager
                .Setup(userManager => userManager.CreateAsync(It.IsAny<UserModel>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _authService.UserRegister(registerModel);

            // Accept
            Assert.False(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CreateAsync(It.IsAny<UserModel>(), registerModel.Password), Times.Once);
        }

        [Fact]
        public async Task UserRegister_SuccessUserRegistration_ReturnTrue()
        {
            // Arrange
            var registerModel = new RegisterModelDTO()
            {
                UserName = "Test",
                Password = "password"
            };

            _mockUserManager
                .Setup(userManager => userManager.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<UserModel>);
            _mockUserManager
                .Setup(userManager => userManager.CreateAsync(It.IsAny<UserModel>(), registerModel.Password))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager
                .Setup(userManager => userManager.AddToRoleAsync(It.IsAny<UserModel>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockJwtGenService
                .Setup(jwtGenService => jwtGenService.GenerateJwtTokens(It.IsAny<List<Claim>>()))
                .Returns(It.IsAny<(string, string)>);
            _mockCookieService
                .Setup(cookieService => cookieService.SetTokens(It.IsAny<string>(), It.IsAny<string>()));
            _mockJwtDataService
                .Setup(jwtDataService => jwtDataService.AddRefreshTokenAsync(It.IsAny<string>(), It.IsAny<UserModel>()));

            // Act
            var result = await _authService.UserRegister(registerModel);

            // Accept
            Assert.True(result);

            _mockUserManager
                .Verify(userManager => userManager.FindByNameAsync(registerModel.UserName), Times.Once);
            _mockUserManager
                .Verify(userManager => userManager.CreateAsync(It.IsAny<UserModel>(), registerModel.Password), Times.Once);

            _mockUserManager
                .Verify(userManager => userManager.AddToRoleAsync(It.IsAny<UserModel>(), It.IsAny<string>()), Times.Once);
            _mockJwtGenService
                .Verify(jwtGenService => jwtGenService.GenerateJwtTokens(It.IsAny<List<Claim>>()), Times.Once);
            _mockCookieService
                .Verify(cookieService => cookieService.SetTokens(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockJwtDataService
                .Verify(jwtDataService => jwtDataService.AddRefreshTokenAsync(It.IsAny<string>(), It.IsAny<UserModel>()), Times.Once);
        }
    }
}
