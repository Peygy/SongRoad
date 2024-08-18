using MainApp.Protos;
using MainApp.Services.Music;
using Moq;

namespace MainApp.Tests.Music.GoogleDriveAppConnectorServiceTests
{
    public abstract class BaseGoogleDriveAppConnectorTests
    {
        protected readonly GoogleDriveAppConnectorService _googleConnectorService;
        protected readonly Mock<GoogleDriveConnector.GoogleDriveConnectorClient> _mockClient;

        public BaseGoogleDriveAppConnectorTests()
        {
            _mockClient = new Mock<GoogleDriveConnector.GoogleDriveConnectorClient>();

            _googleConnectorService = new GoogleDriveAppConnectorService(_mockClient.Object);
        }
    }
}
