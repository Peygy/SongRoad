using MainApp.Models.Music;
using Moq;

namespace MainApp.Tests.MusicServiceTests
{
    public class GetMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task GetMusicTrackByIdAsync_NoExistMusicTrack_ReturnNull()
        {
            // Arrange
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicTrack?)null);

            // Act
            var musicTrackModel = await _musicService.GetMusicTrackByIdAsync(It.IsAny<string>());

            // Assert
            Assert.Null(musicTrackModel);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMusicTrackByIdAsync_ExistMusicTrack_ReturnMusicTrackModelDTO()
        {
            // Arrange
            var musicTrack = new MusicTrack()
            {
                Title = "testTitle",
                Style = new Style() { Name = "testStyle"}
            };
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrack);

            // Act
            var musicTrackModel = await _musicService.GetMusicTrackByIdAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTrackModel);
            Assert.Equal(musicTrack.Title, musicTrackModel.Title);
            Assert.Equal(musicTrack.Style.Name, musicTrackModel.Style);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllMusicTracksAsync_AnyMusicTracks_ReturnCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>()
            {
                new MusicTrack()
                {
                    Title = "testTitle1",
                    Style = new Style() { Name = "testStyle"}
                },
                new MusicTrack()
                {
                    Title = "testTitle2",
                    Style = new Style() { Name = "testStyle"}
                }
            };

            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(musicTrackList);

            // Act
            var musicTrackModelEnumerable = await _musicService.GetAllMusicTracksAsync();

            // Assert
            Assert.NotNull(musicTrackModelEnumerable);
            Assert.NotEmpty(musicTrackModelEnumerable);
            Assert.Equal(musicTrackList.Count, musicTrackModelEnumerable.Count());
            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllMusicTracksAsync_NoAnyMusicTrack_ReturnEmptyCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>();

            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(musicTrackList);

            // Act
            var musicTrackModelEnumerable = await _musicService.GetAllMusicTracksAsync();

            // Assert
            Assert.NotNull(musicTrackModelEnumerable);
            Assert.Empty(musicTrackModelEnumerable);
            Assert.Equal(musicTrackList.Count, musicTrackModelEnumerable.Count());
            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserUploadedTrackListAsync_AnyUploadedMusicTracks_ReturnCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>()
            {
                new MusicTrack()
                {
                    Title = "testTitle1",
                    Style = new Style() { Name = "testStyle"}
                },
                new MusicTrack()
                {
                    Title = "testTitle2",
                    Style = new Style() { Name = "testStyle"}
                }
            };
            string? userId = null;

            _mockMongoService.Setup(m => m.GetUploadedTracksByAuthorIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrackList);

            // Act
            var musicTrackModelEnumerable = await _musicService.GetUserUploadedTrackListAsync(userId!);

            // Assert
            Assert.NotNull(musicTrackModelEnumerable);
            Assert.NotEmpty(musicTrackModelEnumerable);
            Assert.Equal(musicTrackList.Count, musicTrackModelEnumerable.Count());
            _mockMongoService.Verify(m => m.GetUploadedTracksByAuthorIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetUserUploadedTrackListAsync_NoAnyUploadedMusicTrack_ReturnEmptyCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>();
            string? userId = null;

            _mockMongoService.Setup(m => m.GetUploadedTracksByAuthorIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrackList);

            // Act
            var musicTrackModelEnumerable = await _musicService.GetUserUploadedTrackListAsync(userId!);

            // Assert
            Assert.NotNull(musicTrackModelEnumerable);
            Assert.Empty(musicTrackModelEnumerable);
            Assert.Equal(musicTrackList.Count, musicTrackModelEnumerable.Count());
            _mockMongoService.Verify(m => m.GetUploadedTracksByAuthorIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllLikedMusicTracksAsync_AnyLikedMusicTracks_ReturnCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>()
            {
                new MusicTrack()
                {
                    Title = "testTitle1",
                    Style = new Style() { Name = "testStyle"}
                },
                new MusicTrack()
                {
                    Title = "testTitle2",
                    Style = new Style() { Name = "testStyle"}
                }
            };
            string? userId = null;

            _mockMongoService.Setup(m => m.GetLikedTracksByAuthorIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrackList);

            // Act
            var musicTrackModelEnumerable = await _musicService.GetAllLikedMusicTracksAsync(userId!);

            // Assert
            Assert.NotNull(musicTrackModelEnumerable);
            Assert.NotEmpty(musicTrackModelEnumerable);
            Assert.Equal(musicTrackList.Count, musicTrackModelEnumerable.Count());
            _mockMongoService.Verify(m => m.GetLikedTracksByAuthorIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllLikedMusicTracksAsync_NoAnyLikedMusicTrack_ReturnEmptyCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>();
            string? userId = null;

            _mockMongoService.Setup(m => m.GetLikedTracksByAuthorIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrackList);

            // Act
            var musicTrackModelEnumerable = await _musicService.GetAllLikedMusicTracksAsync(userId!);

            // Assert
            Assert.NotNull(musicTrackModelEnumerable);
            Assert.Empty(musicTrackModelEnumerable);
            Assert.Equal(musicTrackList.Count, musicTrackModelEnumerable.Count());
            _mockMongoService.Verify(m => m.GetLikedTracksByAuthorIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMusicStylesAsync_AnyStyles_ReturnCollection()
        {
            // Arrange
            var stylesList = new List<Style>()
            {
                new Style(),
                new Style()
            };

            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).ReturnsAsync(stylesList);

            // Act
            var stylesEnumerable = await _musicService.GetMusicStylesAsync();

            // Assert
            Assert.NotNull(stylesEnumerable);
            Assert.NotEmpty(stylesEnumerable);
            Assert.Equal(stylesList.Count, stylesEnumerable.Count());
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMusicStylesAsync_NoAnyStyles_ReturnEmptyCollection()
        {
            // Arrange
            var stylesList = new List<Style>();

            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).ReturnsAsync(stylesList);

            // Act
            var stylesEnumerable = await _musicService.GetMusicStylesAsync();

            // Assert
            Assert.NotNull(stylesEnumerable);
            Assert.Empty(stylesEnumerable);
            Assert.Equal(stylesList.Count, stylesEnumerable.Count());
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
        }
    }
}
