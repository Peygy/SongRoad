using MainApp.Services.Music;
using Moq;

namespace MainApp.Tests.Music.MusicServiceTests
{
    public class BaseMusicServiceTests
    {
        protected readonly Mock<IMongoService> _mockMongoService;
        protected readonly Mock<IGoogleDriveAppConnectorService> _mockDriveApi;
        protected readonly IMusicService _musicService;

        public BaseMusicServiceTests()
        {
            _mockMongoService = new Mock<IMongoService>();
            _mockDriveApi = new Mock<IGoogleDriveAppConnectorService>();
            _musicService = new MusicService(_mockMongoService.Object, _mockDriveApi.Object);
        }
    }
}
