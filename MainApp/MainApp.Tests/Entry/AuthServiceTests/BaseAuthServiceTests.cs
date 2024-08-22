using MainApp.Models.User;
using MainApp.Services.Entry;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MainApp.Tests.Entry.AuthServiceTests
{
    public class BaseAuthServiceTests
    {
        protected readonly Mock<UserManager<UserModel>> _mockUserManager;
        protected readonly Mock<IJwtGenService> _mockJwtGenService;
        protected readonly Mock<IRefershTokenService> _mockJwtDataService;
        protected readonly Mock<ICookieService> _mockCookieService;

        protected readonly AuthService _authService;

        public BaseAuthServiceTests()
        {
            _mockUserManager = new Mock<UserManager<UserModel>>(
                new Mock<IUserStore<UserModel>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<UserModel>>().Object,
                new IUserValidator<UserModel>[0],
                new IPasswordValidator<UserModel>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<UserModel>>>().Object
            );

            _mockUserManager.Setup(m => m.GetRolesAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(new List<string> { "Role1", "Role2" });

            _mockJwtGenService = new Mock<IJwtGenService>();
            _mockJwtDataService = new Mock<IRefershTokenService>();
            _mockCookieService = new Mock<ICookieService>();

            _authService = new AuthService(
                _mockUserManager.Object,
                _mockJwtGenService.Object,
                _mockJwtDataService.Object,
                _mockCookieService.Object
            );
        }
    }
}
