using MainApp.Services.Entry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MainApp.Tests.Entry.CookieServiceTests
{
    public class BaseCookieServiceTests
    {
        protected readonly ICookieService _cookieService;
        protected readonly Mock<ILogger<CookieService>> _logger;
        protected readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        protected readonly Mock<IRequestCookieCollection> _mockRequestCookies;
        protected readonly Mock<IResponseCookies> _mockResponseCookies;

        public BaseCookieServiceTests()
        {
            _mockRequestCookies = new Mock<IRequestCookieCollection>();
            _mockResponseCookies = new Mock<IResponseCookies>();

            var requestMock = new Mock<HttpRequest>();
            requestMock.Setup(x => x.Cookies).Returns(_mockRequestCookies.Object);

            var responseMock = new Mock<HttpResponse>();
            responseMock.Setup(x => x.Cookies).Returns(_mockResponseCookies.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);
            httpContextMock.Setup(x => x.Response).Returns(responseMock.Object);

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

            _logger = new Mock<ILogger<CookieService>>();
            _cookieService = new CookieService(_logger.Object, _mockHttpContextAccessor.Object);
        }
    }
}
