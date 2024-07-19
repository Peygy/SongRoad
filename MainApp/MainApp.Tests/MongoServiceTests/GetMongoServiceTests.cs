using MainApp.Models.Music;
using MainApp.Models.User;
using MongoDB.Driver;

namespace MainApp.Tests.MongoServiceTests
{
    public class GetMongoServiceTests : BaseMongoServiceTests
    {
        public GetMongoServiceTests(WebAppFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task GetAuthorByIdAsync_AuthorExists_ReturnsAuthor()
        {
            // Arrange
            var user = new UserModel { Id = "Author1", UserName = "ExistAuthor" };
            await _musicAuthorsCollection.InsertOneAsync(new MusicAuthor { Id = user.Id, Name = user.UserName });

            //Act
            var result = await _mongoService.GetAuthorByIdAsync(user.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.UserName, result.Name);
        }

        [Fact]
        public async Task GetAuthorByIdAsync_AuthorNotExists_ReturnsNull()
        {
            // Arrange
            var user = new UserModel { Id = "Author2", UserName = "NotExistAuthor" };

            //Act
            var result = await _mongoService.GetAuthorByIdAsync(user.Id);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTrackByIdAsync_TrackExists_ReturnsTrack()
        {
            // Arrange
            var track = new MusicTrack { Title = "ExistTrack", CreatorId = "Author3" };
            await _tracksCollection.InsertOneAsync(track);

            //Act
            var result = await _mongoService.GetTrackByIdAsync(track.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(track.Title, result.Title);
            Assert.Equal(track.CreatorId, result.CreatorId);
        }

        [Fact]
        public async Task GetTrackByIdAsync_TrackNotExists_ReturnsNull()
        {
            // Arrange
            var track = new MusicTrack { Title = "NotExistTrack", CreatorId = "Author4" };

            //Act
            var result = await _mongoService.GetTrackByIdAsync(track.Id);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMusicStylesAsync_ListNotNull_ReturnsList()
        {
            //Act
            var result = await _mongoService.GetMusicStylesAsync();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            var styleName = "pop";
            var style = result.Where(s => s.Name == styleName).FirstOrDefault();
            Assert.NotNull(style);
            Assert.Equal(styleName, style.Name);
        }

        [Fact]
        public async Task GetAllTracksAsync_ListNotNull_ReturnsList()
        {
            // Arrange
            await _tracksCollection.DeleteManyAsync(Builders<MusicTrack>.Filter.Empty);

            await _tracksCollection.InsertManyAsync(new List<MusicTrack> {
                new MusicTrack { Title = "ExistTrack1", CreatorId = "Author5" },
                new MusicTrack { Title = "ExistTrack2", CreatorId = "Author5" }
            });

            //Act
            var result = await _mongoService.GetAllTracksAsync();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.Title == "ExistTrack1");
            Assert.Contains(result, t => t.Title == "ExistTrack2");
        }

        [Fact]
        public async Task GetAllTracksAsync_ListIsNull_ReturnsNull()
        {
            // Arrange
            await _tracksCollection.DeleteManyAsync(Builders<MusicTrack>.Filter.Empty);

            //Act
            var result = await _mongoService.GetAllTracksAsync();

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
