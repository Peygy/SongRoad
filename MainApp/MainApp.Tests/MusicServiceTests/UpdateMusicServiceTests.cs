using MainApp.Models.Music;
using MainApp.DTO.Music;
using Moq;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace MainApp.Tests.MusicServiceTests
{
    public class UpdateMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task UpdateMusicTrackAsync_ShouldUpdateTrack_WhenTrackExists()
        {
            // Arrange
            var styles = new List<Style>
            {
                new Style { Id = ObjectId.Parse("507c7f79bcf86cd7994f6c0e"), Name = "oldStyle" },
                new Style { Id = ObjectId.Parse("507c7f79bcf86cd7994f6c1e"), Name = "newStyle" }
            };
            var newTestTrack = new NewMusicTrackModelDTO()
            {
                Title = "testTrackNew",
                StyleId = styles[1].Id.ToString(),
                Mp3File = new FormFile(new MemoryStream([ 1, 2, 3 ]), 0, 3, "Mp3File", "Mp3File.mp3")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "audio/mpeg"
                }
            };
            var testTrack = new MusicTrack()
            {
                Id = ObjectId.Parse("507c7f79bcf86cd7994f6c2e"),
                Title = "testTrack",
                Style = styles[0],
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };

            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(testTrack);
            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).ReturnsAsync(styles);
            _mockMongoService.Setup(m => m.UpdateTrackAsync(testTrack)).ReturnsAsync(true);
            _mockDriveApi.Setup(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()));

            // Act
            var result = await _musicService.UpdateMusicTrackAsync(testTrack.Id.ToString(), newTestTrack);

            // Assert
            Assert.True(result);
            Assert.Equal(newTestTrack.Title, testTrack.Title);
            Assert.Equal(newTestTrack.StyleId, testTrack.Style.Id.ToString());

            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
            _mockDriveApi.Verify(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMusicTrackAsync_ShouldNotUpdateMp3File_WhenMp3FileIsNullOrEmpty()
        {
            // Arrange
            var styles = new List<Style>
            {
                new Style { Id = ObjectId.Parse("507c7f79bcf86cd7994f6c0e"), Name = "oldStyle" },
                new Style { Id = ObjectId.Parse("507c7f79bcf86cd7994f6c1e"), Name = "newStyle" }
            };
            var newTestTrack = new NewMusicTrackModelDTO()
            {
                Title = "testTrackNew",
                StyleId = styles[1].Id.ToString()
            };
            var testTrack = new MusicTrack()
            {
                Id = ObjectId.Parse("507c7f79bcf86cd7994f6c2e"),
                Title = "testTrack",
                Style = styles[0],
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };

            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(testTrack);
            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).ReturnsAsync(styles);
            _mockMongoService.Setup(m => m.UpdateTrackAsync(testTrack)).ReturnsAsync(true);

            // Act
            var result = await _musicService.UpdateMusicTrackAsync(testTrack.Id.ToString(), newTestTrack);

            // Assert
            Assert.True(result);
            Assert.Equal(newTestTrack.Title, testTrack.Title);
            Assert.Equal(newTestTrack.StyleId, testTrack.Style.Id.ToString());

            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
            _mockDriveApi.Verify(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateMusicTrackAsync_ShouldNotUpdateTrack_WhenTrackNotExists()
        {
            // Arrange
            var testTrackId = "testTrackId";
            var newTestTrack = new NewMusicTrackModelDTO();
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicTrack?)null);

            // Act
            var result = await _musicService.UpdateMusicTrackAsync(testTrackId, newTestTrack);

            // Assert
            Assert.False(result);

            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Never);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Never);
            _mockDriveApi.Verify(m => m.UpdateFile(It.IsAny<FormFile>(), It.IsAny<string>()), Times.Never);
        }
    }
}
