using MainApp.DTO.Music;
using Moq;

namespace MainApp.Tests.MusicServiceTests
{
    public class AddMusicServiceTests
    {
        [Fact]
        public async Task AddTrackAsync_ShouldReturnTrue_WhenTrackIsAddedSuccessfully()
        {
            // Arrange
            /*var musicTrackModel = new NewMusicTrackModelDTO
            {
                Title = "Test Track",
                TrackImage = new byte[] { 0x20, 0x20 },
                Mp3File = new byte[] { 0x30, 0x30 },
                Style = "Rock"
            };
            var userId = "user123";
            var imageModel = new ImageModel { Url = "http://example.com/image.png" };
            var trackId = "track123";

            _mockMongoService.Setup(m => m.AddMusicTrackImageAsync(It.IsAny<byte[]>()))
                .ReturnsAsync(imageModel);
            _mockMongoService.Setup(m => m.AddNewTrackAsync(It.IsAny<MusicTrack>(), It.IsAny<string>()))
                .ReturnsAsync(trackId);
            _mockDriveApi.Setup(d => d.UploadFile(It.IsAny<byte[]>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _musicService.AddTrackAsync(musicTrackModel, userId);

            // Assert
            Assert.True(result);
            _mockMongoService.Verify(m => m.AddMusicTrackImageAsync(It.IsAny<byte[]>()), Times.Once);
            _mockMongoService.Verify(m => m.AddNewTrackAsync(It.IsAny<MusicTrack>(), It.IsAny<string>()), Times.Once);
            _mockDriveApi.Verify(d => d.UploadFile(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Once);*/
        }
    }
}
