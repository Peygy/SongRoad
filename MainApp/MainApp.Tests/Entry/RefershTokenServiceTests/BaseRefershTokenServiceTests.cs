using MainApp.Data;
using MainApp.Services.Entry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace MainApp.Tests.Entry.RefershTokenServiceTests
{
    public class BaseRefershTokenServiceTests : IClassFixture<UserContextWepAppFactory>
    {
        protected readonly IRefershTokenService _refershTokenService;
        protected readonly Mock<ILogger<RefershTokenService>> _logger;
        protected readonly UserContext _userContext;
        protected readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        protected readonly Mock<ConnectionInfo> _mockConnection;

        public BaseRefershTokenServiceTests(UserContextWepAppFactory factory)
        {
            var httpContextMock = new Mock<HttpContext>();
            _mockConnection = new Mock<ConnectionInfo>();
            _mockConnection.Setup(c => c.RemoteIpAddress).Returns(IPAddress.Parse("127.0.0.1"));
            httpContextMock.Setup(x => x.Connection).Returns(_mockConnection.Object);

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            var _scope = factory.Services.CreateScope();
            _userContext = _scope.ServiceProvider.GetRequiredService<UserContext>();
            _logger = new Mock<ILogger<RefershTokenService>>();

            _refershTokenService = new RefershTokenService(_logger.Object, _userContext, _mockHttpContextAccessor.Object);
        }
    }
}
