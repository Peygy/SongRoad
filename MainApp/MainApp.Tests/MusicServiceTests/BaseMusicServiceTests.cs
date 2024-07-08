using MainApp.Interfaces.Music;
using MainApp.Services;
using Moq;

namespace MainApp.Tests.MusicServiceTests
{
    public class BaseMusicServiceTests
    {
        protected readonly Mock<IMongoService> _mockMongoService;
        protected readonly Mock<IGoogleDriveApi> _mockDriveApi;
        protected readonly IMusicService _musicService;

        public BaseMusicServiceTests()
        {
            _mockMongoService = new Mock<IMongoService>();
            _mockDriveApi = new Mock<IGoogleDriveApi>();
            _musicService = new MusicService(_mockMongoService.Object, _mockDriveApi.Object);
        }
    }
}
