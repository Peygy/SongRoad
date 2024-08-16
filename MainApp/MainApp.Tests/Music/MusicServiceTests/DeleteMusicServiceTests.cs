using Moq;

namespace MainApp.Tests.Music.MusicServiceTests
{
    public class DeleteMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task DeleteMusicTrackAsync_ShouldDeleteTrackAndFile_ReturnTrue()
        {
            // Arrange
            _mockMongoService.Setup(m => m.DeleteTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockDriveApi.Setup(m => m.DeleteFile(It.IsAny<string>())).ReturnsAsync(true);

            // Act
            var result = await _musicService.DeleteMusicTrackAsync(It.IsAny<string>());

            // Assert
            Assert.True(result);
            _mockMongoService.Verify(m => m.DeleteTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockDriveApi.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMusicTrackAsync_ShouldDeleteOnlyTrack_ReturnFalse()
        {
            // Arrange
            _mockMongoService.Setup(m => m.DeleteTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(true);
            _mockDriveApi.Setup(m => m.DeleteFile(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _musicService.DeleteMusicTrackAsync(It.IsAny<string>());

            // Assert
            Assert.False(result);
            _mockMongoService.Verify(m => m.DeleteTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockDriveApi.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteMusicTrackAsync_ShouldNotDeleteAny_ReturnFalse()
        {
            // Arrange
            _mockMongoService.Setup(m => m.DeleteTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(false);

            // Act
            var result = await _musicService.DeleteMusicTrackAsync(It.IsAny<string>());

            // Assert
            Assert.False(result);
            _mockMongoService.Verify(m => m.DeleteTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockDriveApi.Verify(m => m.DeleteFile(It.IsAny<string>()), Times.Never);
        }
    }
}
