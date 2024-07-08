using MainApp.Models.Music;
using MainApp.DTO.Music;
using Moq;
using Microsoft.AspNetCore.Http;

namespace MainApp.Tests.MusicServiceTests
{
    public class UpdateMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task UpdateMusicTrackAsync_ShouldUpdateTrack_WhenTrackExists()
        {
            // Arrange
            var newTestTrack = new NewMusicTrackModelDTO()
            {
                Title = "testTrackNew",
                Style = "newStyleId",
                TrackImage = new FormFile(new MemoryStream([ 1, 2, 3 ]), 0, 3, "TrackImage", "TrackImage.jpg")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                },
                Mp3File = new FormFile(new MemoryStream([ 1, 2, 3 ]), 0, 3, "Mp3File", "Mp3File.mp3")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "audio/mpeg"
                }
            };
            var testTrack = new MusicTrack()
            {
                Id = "testTrack",
                Title = "testTrack",
                Style = new Style { Id = "oldStyleId", Name = "oldStyle" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var styles = new List<Style>
            {
                new Style { Id = "oldStyleId", Name = "oldStyle" },
                new Style { Id = "newStyleId", Name = "newStyle" }
            };

            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(testTrack);
            _mockMongoService.Setup(m => m.UpdateMusicTrackImageAsync(It.IsAny<MusicTrack>(), It.IsAny<FormFile>())).Returns(Task.CompletedTask);
            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).Returns(() => Task.FromResult(styles));
            _mockMongoService.Setup(m => m.UpdateTrackByIdAsync(It.IsAny<MusicTrack>()));
            _mockDriveApi.Setup(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()));

            // Act
            await _musicService.UpdateMusicTrackAsync(testTrack.Id, newTestTrack);

            // Assert
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.UpdateMusicTrackImageAsync(It.IsAny<MusicTrack>(), It.IsAny<FormFile>()), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
            _mockMongoService.Verify(m => m.UpdateTrackByIdAsync(It.IsAny<MusicTrack>()), Times.Once);
            _mockDriveApi.Verify(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()), Times.Once);

            Assert.Equal(newTestTrack.Title, testTrack.Title);
            var style = styles.FirstOrDefault(s => s.Id == newTestTrack.Style);
            Assert.Equal(style, testTrack.Style);
        }

        [Fact]
        public async Task UpdateMusicTrackAsync_ShouldNotUpdateTrack_TrackDoesNotExist_ThrowsException()
        {
            // Arrange
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicTrack)null);

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () => 
                await _musicService.UpdateMusicTrackAsync("testTrackId", new NewMusicTrackModelDTO()));

            // Assert
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.UpdateMusicTrackImageAsync(It.IsAny<MusicTrack>(), It.IsAny<FormFile>()), Times.Never);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Never);
            _mockMongoService.Verify(m => m.UpdateTrackByIdAsync(It.IsAny<MusicTrack>()), Times.Never);
            _mockDriveApi.Verify(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()), Times.Never);

            Assert.Equal("Track not found", exception.Message);
        }

        [Fact]
        public async Task UpdateMusicTrackAsync_ShouldNotUpdateMp3File_WhenMp3FileIsNullOrEmpty()
        {
            // Arrange
            var newTestTrack = new NewMusicTrackModelDTO()
            {
                Title = "testTrackNew",
                Style = "newStyleId",
                TrackImage = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "TrackImage", "TrackImage.jpg")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/jpeg"
                }
            };
            var testTrack = new MusicTrack()
            {
                Id = "testTrack",
                Title = "testTrack",
                Style = new Style { Id = "oldStyleId", Name = "oldStyle" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var styles = new List<Style>
            {
                new Style { Id = "oldStyleId", Name = "oldStyle" },
                new Style { Id = "newStyleId", Name = "newStyle" }
            };

            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(testTrack);
            _mockMongoService.Setup(m => m.UpdateMusicTrackImageAsync(It.IsAny<MusicTrack>(), It.IsAny<FormFile>())).Returns(Task.CompletedTask);
            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).Returns(() => Task.FromResult(styles));
            _mockMongoService.Setup(m => m.UpdateTrackByIdAsync(It.IsAny<MusicTrack>()));

            // Act
            await _musicService.UpdateMusicTrackAsync(testTrack.Id, newTestTrack);

            // Assert
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.UpdateMusicTrackImageAsync(It.IsAny<MusicTrack>(), It.IsAny<FormFile>()), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
            _mockMongoService.Verify(m => m.UpdateTrackByIdAsync(It.IsAny<MusicTrack>()), Times.Once);
            _mockDriveApi.Verify(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()), Times.Never);

            Assert.Equal(newTestTrack.Title, testTrack.Title);
            var style = styles.FirstOrDefault(s => s.Id == newTestTrack.Style);
            Assert.Equal(style, testTrack.Style);
        }
    }
}
