using MainApp.Data;
using MainApp.Models.User;
using MainApp.Services.Crew;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MainApp.Tests.Crew.UserManageServiceTests
{
    public class BaseUserManageServiceTests : IClassFixture<UserContextWepAppFactory>
    {
        protected readonly Mock<UserManager<UserModel>> _mockUserManager;
        protected readonly UserContext _userContext;
        protected readonly UserManageService _userManageService;

        public BaseUserManageServiceTests(UserContextWepAppFactory factory)
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

            var _scope = factory.Services.CreateScope();
            _userContext = _scope.ServiceProvider.GetRequiredService<UserContext>();

            _userManageService = new UserManageService(_mockUserManager.Object, _userContext);
        }
    }
}
