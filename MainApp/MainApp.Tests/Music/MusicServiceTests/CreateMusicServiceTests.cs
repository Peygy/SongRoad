using MainApp.DTO.Music;
using MainApp.Models.Music;
using MainApp.Services.Music;
using Moq;
using System.Reflection;

namespace MainApp.Tests.Music.MusicServiceTests
{
    public class CreateMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task CreateMusicDTOCollection_AddTracksWithLikedFlags_ReturnCollection()
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
            var currentAuthor = new MusicAuthor();
            currentAuthor.LikedTracks.Add(musicTrackList[0].Id);
            var userId = "userId";

            _mockMongoService.SetupSequence(m => m.GetAuthorByIdAsync(userId))
                .ReturnsAsync(currentAuthor)
                .ReturnsAsync(currentAuthor);

            var musicService = new MusicService(_mockMongoService.Object, _mockDriveApi.Object);
            var method = typeof(MusicService).GetMethod("CreateMusicDTOCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { musicTrackList, userId };

            // Act
            var result = await (Task<IEnumerable<MusicTrackModelDTO>>)method.Invoke(musicService, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(userId), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateMusicDTOCollection_AddTracksWithOutLikedFlags_ReturnCollection()
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

            var musicService = new MusicService(_mockMongoService.Object, _mockDriveApi.Object);
            var method = typeof(MusicService).GetMethod("CreateMusicDTOCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { musicTrackList, null! };

            // Act
            var result = await (Task<IEnumerable<MusicTrackModelDTO>>)method.Invoke(musicService, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CreateMusicDTOCollection_NoAddTracks_ReturnEmptyCollection()
        {
            // Arrange
            var musicTrackList = new List<MusicTrack>();

            var musicService = new MusicService(_mockMongoService.Object, _mockDriveApi.Object);
            var method = typeof(MusicService).GetMethod("CreateMusicDTOCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { musicTrackList, null! };

            // Act
            var result = await (Task<IEnumerable<MusicTrackModelDTO>>)method.Invoke(musicService, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockMongoService.Verify(m => m.GetAuthorByIdAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
