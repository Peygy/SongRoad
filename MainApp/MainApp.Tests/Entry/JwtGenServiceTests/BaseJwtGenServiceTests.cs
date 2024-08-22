using MainApp.Services.Entry;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MainApp.Tests.Entry.JwtGenServiceTests
{
    public class BaseJwtGenServiceTests
    {
        protected readonly JwtGenService _jwtGenService;
        protected readonly Mock<IConfiguration> _mockConfiguration;

        public BaseJwtGenServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.SetupGet(x => x["JwtSettings:KEY"]).Returns("4vm8tuv09jvilslrxd90rumt30rutb308t");
            _mockConfiguration.SetupGet(x => x["JwtSettings:ISSUER"]).Returns("TestIssuer");
            _mockConfiguration.SetupGet(x => x["JwtSettings:AUDIENCE"]).Returns("TestAudience");

            _jwtGenService = new JwtGenService(_mockConfiguration.Object);
        }
    }
}
