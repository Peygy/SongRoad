using MainApp.DTO.Music;
using MainApp.Models.Music;
using Moq;

namespace MainApp.Tests.MusicServiceTests
{
    public class GetMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task GetUserUploadedTrackListAsync_ShouldReturnListOfTracks_WhenTracksExist()
        {
            // Arrange
            string userId = "testUserId";
            var authorModel = new MusicAuthor()
            {
                Id = userId,
                Name = "testUser",
                UploadedTracksId = new List<string> { "testTrack1", "testTrack2" },
                LikedTracksId = new List<string> { "testTrack1" }
            };
            var testTrack1 = new MusicTrack() {
                Id = "testTrack1",
                Title = "testTrack1",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var testTrack2 = new MusicTrack()
            {
                Id = "testTrack2",
                Title = "testTrack2",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };

            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync(authorModel);
            _mockMongoService.SetupSequence(m => m.GetTrackByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(testTrack1)
                .ReturnsAsync(testTrack2);

            // Act
            var musicTracks = await _musicService.GetUserUploadedTrackListAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTracks);
            Assert.NotEmpty(musicTracks);
            Assert.Equal(2, musicTracks.Count);

            Assert.Equal(testTrack1.Title, musicTracks[0].Title);
            Assert.Equal(testTrack1.Style.Name, musicTracks[0].Style);
            Assert.Equal(testTrack1.Id, musicTracks[0].MusicId);
            Assert.Equal(authorModel.Name, musicTracks[0].CreatorName);
            Assert.True(musicTracks[0].isLiked);
            Assert.Equal(Convert.ToBase64String(testTrack1.TrackImage.ImageData), musicTracks[0].ImageBase64);

            Assert.Equal(testTrack2.Title, musicTracks[1].Title);
            Assert.Equal(testTrack2.Style.Name, musicTracks[1].Style);
            Assert.Equal(testTrack2.Id, musicTracks[1].MusicId);
            Assert.Equal(authorModel.Name, musicTracks[1].CreatorName);
            Assert.False(musicTracks[1].isLiked);
            Assert.Equal(Convert.ToBase64String(testTrack2.TrackImage.ImageData), musicTracks[1].ImageBase64);

            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetUserUploadedTrackListAsync_ShouldReturnEmptyList_NullAuthor()
        {
            // Arrange
            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicAuthor)null);

            // Act
            var musicTracks = await _musicService.GetUserUploadedTrackListAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTracks);
            Assert.Empty(musicTracks);

            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetUserUploadedTrackListAsync_ShouldReturnEmptyList_NullTracks()
        {
            // Arrange
            var authorModel = new MusicAuthor()
            {
                UploadedTracksId = new List<string> { "testTrack1", "testTrack2" }
            };

            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync(authorModel);
            _mockMongoService.SetupSequence(m => m.GetTrackByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((MusicTrack)null)
                .ReturnsAsync((MusicTrack)null);

            // Act
            var musicTracks = await _musicService.GetUserUploadedTrackListAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTracks);
            Assert.Empty(musicTracks);

            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }


        [Fact]
        public async Task GetMusicTrackByIdAsync_NoExistMusicTrack_ReturnNull()
        {
            // Arrange
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicTrack)null);

            // Act
            var result_musicTrackModel = await _musicService.GetMusicTrackByIdAsync<MusicTrack>(It.IsAny<string>());

            // Assert
            Assert.Null(result_musicTrackModel);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMusicTrackByIdAsync_NoExistMusicTrack_ReturnMusicTrackModelDTO()
        {
            // Arrange
            var musicTrackModel = new MusicTrack()
            {
                Id = "testTrack",
                Title = "testTrack",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrackModel);

            // Act
            var result_musicTrackModel = await _musicService.GetMusicTrackByIdAsync<MusicTrackModelDTO>(It.IsAny<string>());

            // Assert
            Assert.NotNull(result_musicTrackModel);
            Assert.IsType<MusicTrackModelDTO>(result_musicTrackModel);

            Assert.Equal(musicTrackModel.Title, result_musicTrackModel.Title);
            Assert.Equal(musicTrackModel.Style.Name, result_musicTrackModel.Style);
            Assert.Equal(musicTrackModel.Id, result_musicTrackModel.MusicId);
            Assert.Equal(Convert.ToBase64String(musicTrackModel.TrackImage.ImageData), result_musicTrackModel.ImageBase64);

            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMusicTrackByIdAsync_NoExistMusicTrack_ReturnMusicTrack()
        {
            // Arrange
            var musicTrackModel = new MusicTrack()
            {
                Id = "testTrack",
                Title = "testTrack",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            _mockMongoService.Setup(m => m.GetTrackByIdAsync(It.IsAny<string>())).ReturnsAsync(musicTrackModel);

            // Act
            var result_musicTrackModel = await _musicService.GetMusicTrackByIdAsync<MusicTrack>(It.IsAny<string>());

            // Assert
            Assert.NotNull(result_musicTrackModel);
            Assert.IsType<MusicTrack>(result_musicTrackModel);

            Assert.Equal(musicTrackModel.Title, result_musicTrackModel.Title);
            Assert.Equal(musicTrackModel.Style.Name, result_musicTrackModel.Style.Name);
            Assert.Equal(musicTrackModel.Id, result_musicTrackModel.Id);
            Assert.Equal(Convert.ToBase64String(musicTrackModel.TrackImage.ImageData), 
                Convert.ToBase64String(result_musicTrackModel.TrackImage.ImageData));

            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task GetAllLikedMusicTracksAsync_ShouldReturnListOfLikedTracks_WhenTracksExist()
        {
            // Arrange
            var authorModel = new MusicAuthor()
            {
                Name = "testUser",
                LikedTracksId = new List<string> { "testTrack1", "testTrack2" }
            };
            var testTrack1 = new MusicTrack()
            {
                Id = "testTrack1",
                Title = "testTrack1",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var testTrack2 = new MusicTrack()
            {
                Id = "testTrack2",
                Title = "testTrack2",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };

            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync(authorModel);
            _mockMongoService.SetupSequence(m => m.GetTrackByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(testTrack1)
                .ReturnsAsync(testTrack2);

            // Act
            var musicTracks = await _musicService.GetAllLikedMusicTracksAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTracks);
            Assert.NotEmpty(musicTracks);
            Assert.Equal(2, musicTracks.Count);

            Assert.Equal(testTrack1.Title, musicTracks[0].Title);
            Assert.Equal(testTrack1.Style.Name, musicTracks[0].Style);
            Assert.Equal(testTrack1.Id, musicTracks[0].MusicId);
            Assert.Equal(authorModel.Name, musicTracks[0].CreatorName);
            Assert.True(musicTracks[0].isLiked);
            Assert.Equal(Convert.ToBase64String(testTrack1.TrackImage.ImageData), musicTracks[0].ImageBase64);

            Assert.Equal(testTrack2.Title, musicTracks[1].Title);
            Assert.Equal(testTrack2.Style.Name, musicTracks[1].Style);
            Assert.Equal(testTrack2.Id, musicTracks[1].MusicId);
            Assert.Equal(authorModel.Name, musicTracks[1].CreatorName);
            Assert.True(musicTracks[1].isLiked);
            Assert.Equal(Convert.ToBase64String(testTrack2.TrackImage.ImageData), musicTracks[1].ImageBase64);

            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetAllLikedMusicTracksAsync_ShouldReturnEmptyList_NullAuthor()
        {
            // Arrange
            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicAuthor)null);

            // Act
            var musicTracks = await _musicService.GetAllLikedMusicTracksAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTracks);
            Assert.Empty(musicTracks);

            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllLikedMusicTracksAsync_ShouldReturnEmptyList_NullTracks()
        {
            // Arrange
            var authorModel = new MusicAuthor()
            {
                Name = "testUser",
                LikedTracksId = new List<string> { "testTrack1", "testTrack2" }
            };

            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync(authorModel);
            _mockMongoService.SetupSequence(m => m.GetTrackByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((MusicTrack)null)
                .ReturnsAsync((MusicTrack)null);

            // Act
            var musicTracks = await _musicService.GetAllLikedMusicTracksAsync(It.IsAny<string>());

            // Assert
            Assert.NotNull(musicTracks);
            Assert.Empty(musicTracks);

            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
            _mockMongoService.Verify(m => m.GetTrackByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }


        [Fact]
        public async Task GetAllMusicTracksAsync_ReturnListOfAllMusicTracks()
        {
            // Arrange
            var tracksList = new List<MusicTrack>() { It.IsAny<MusicTrack>(), It.IsAny<MusicTrack>() };
            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(tracksList);

            // Act
            var result = await _musicService.GetAllMusicTracksAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(tracksList, result);
            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
        }
        [Fact]
        public async Task GetMusicStylesAsync_ReturnListOfAllStyles()
        {
            // Arrange
            var styles = new List<Style>() { It.IsAny<Style>(), It.IsAny<Style>() };
            _mockMongoService.Setup(m => m.GetMusicStylesAsync()).ReturnsAsync(styles);

            // Act
            var result = await _musicService.GetMusicStylesAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(styles, result);
            _mockMongoService.Verify(m => m.GetMusicStylesAsync(), Times.Once);
        }


        [Fact]
        public async Task GetMusicTracksForViewAsync_ShouldReturnTracksWithAuthorInfo_UserIdNotNull()
        {
            // Arrange
            var authorModel = new MusicAuthor()
            {
                Id = "testUserId",
                Name = "testUser",
                LikedTracksId = new List<string> { "testTrack2" }
            };

            var testTrack1 = new MusicTrack()
            {
                Id = "testTrack1",
                Title = "testTrack1",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var testTrack2 = new MusicTrack()
            {
                Id = "testTrack2",
                Title = "testTrack2",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var tracksList = new List<MusicTrack>() { testTrack1, testTrack2 };

            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(tracksList);
            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync(authorModel);

            // Act
            var tracks = await _musicService.GetMusicTracksForViewAsync(authorModel.Id);

            // Accept
            Assert.NotNull(tracks);
            Assert.NotEmpty(tracks);
            Assert.Equal(2, tracks.Count);

            Assert.False(tracks[0].isLiked);
            Assert.Equal(authorModel.Name, tracks[0].CreatorName);
            Assert.True(tracks[1].isLiked);
            Assert.Equal(authorModel.Name, tracks[1].CreatorName);

            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMusicTracksForViewAsync_ShouldReturnTracksWithoutAuthorInfo_UserIdIsNull()
        {
            // Arrange
            var userId = (string)null;
            var testTrack1 = new MusicTrack()
            {
                Id = "testTrack1",
                Title = "testTrack1",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var testTrack2 = new MusicTrack()
            {
                Id = "testTrack2",
                Title = "testTrack2",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var tracksList = new List<MusicTrack>() { testTrack1, testTrack2 };

            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(tracksList);

            // Act
            var tracks = await _musicService.GetMusicTracksForViewAsync(userId);

            // Accept
            Assert.NotNull(tracks);
            Assert.NotEmpty(tracks);
            Assert.Equal(2, tracks.Count);

            Assert.False(tracks[0].isLiked);
            Assert.Null(tracks[0].CreatorName);
            Assert.False(tracks[1].isLiked);
            Assert.Null(tracks[1].CreatorName);

            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(userId), Times.Never);
        }

        [Fact]
        public async Task GetMusicTracksForViewAsync_ShouldHandleNoAuthorFound_UserIdNotNull()
        {
            // Arrange
            var userId = "testUserId";
            var testTrack1 = new MusicTrack()
            {
                Id = "testTrack1",
                Title = "testTrack1",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var testTrack2 = new MusicTrack()
            {
                Id = "testTrack2",
                Title = "testTrack2",
                Style = new Style() { Name = "pop" },
                TrackImage = new TrackImageModel { ImageData = [1, 2, 3] }
            };
            var tracksList = new List<MusicTrack>() { testTrack1, testTrack2 };

            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(tracksList);
            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync((MusicAuthor)null);

            // Act
            var tracks = await _musicService.GetMusicTracksForViewAsync(userId);

            // Accept
            Assert.NotNull(tracks);
            Assert.NotEmpty(tracks);
            Assert.Equal(2, tracks.Count);

            Assert.False(tracks[0].isLiked);
            Assert.Null(tracks[0].CreatorName);
            Assert.False(tracks[1].isLiked);
            Assert.Null(tracks[1].CreatorName);

            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetMusicTracksForViewAsync_ShouldReturnEmptyList_WhenNoTracksExist()
        {
            // Arrange
            var authorModel = new MusicAuthor()
            {
                Id = "testUserId",
                Name = "testUser"
            };
            var tracksList = new List<MusicTrack>();

            _mockMongoService.Setup(m => m.GetAllTracksAsync()).ReturnsAsync(tracksList);
            _mockMongoService.Setup(m => m.GetAuthorByIdAsync(It.IsAny<string>())).ReturnsAsync(authorModel);

            // Act
            var tracks = await _musicService.GetMusicTracksForViewAsync(authorModel.Id);

            // Accept
            Assert.NotNull(tracks);
            Assert.Empty(tracks);

            _mockMongoService.Verify(m => m.GetAllTracksAsync(), Times.Once);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
