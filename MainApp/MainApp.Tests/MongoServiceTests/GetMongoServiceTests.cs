using MainApp.Models.Music;
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
            var user = new MusicAuthor { Id = "Author1", Name = "ExistAuthor" };
            _musicContext.MusicAuthors.Add(user);
            await _musicContext.SaveChangesAsync();

            //Act
            var result = await _mongoService.GetAuthorByIdAsync(user.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task GetAuthorByIdAsync_AuthorNotExists_ReturnsNull()
        {
            // Arrange
            var userId = "Author2";

            //Act
            var result = await _mongoService.GetAuthorByIdAsync(userId);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUploadedTracksByAuthorIdAsync_AnyUploadedTracks_ReturnTracks()
        {
            // Arrange
            var user = new MusicAuthor { Id = "Author3_1", Name = "ExistAuthor" };
            _musicContext.MusicAuthors.Add(user);

            _musicContext.MusicTracks.AddRange([
                new MusicTrack { Title = "ExistTrack1", CreatorId = "Author3_1" },
                new MusicTrack { Title = "ExistTrack2", CreatorId = "Author3_1" }
            ]);
            await _musicContext.SaveChangesAsync();

            //Act
            var uploadedTracks = await _mongoService.GetUploadedTracksByAuthorIdAsync(user.Id);

            //Assert
            Assert.NotNull(uploadedTracks);
            Assert.NotEmpty(uploadedTracks);
            Assert.Equal(2, uploadedTracks.Count());

            Assert.Contains(uploadedTracks, t => t.Title == "ExistTrack1");
            Assert.Contains(uploadedTracks, t => t.Title == "ExistTrack1");
            Assert.Contains(uploadedTracks, t => t.CreatorId == user.Id);
        }

        [Fact]
        public async Task GetUploadedTracksByAuthorIdAsync_NoUploadedTracks_ReturnTracks()
        {
            // Arrange
            var userId = "Author4";

            //Act
            var uploadedTracks = await _mongoService.GetUploadedTracksByAuthorIdAsync(userId);

            //Assert
            Assert.NotNull(uploadedTracks);
            Assert.Empty(uploadedTracks);
        }

        [Fact]
        public async Task GetLikedTracksByAuthorIdAsync_AnyLikedTracks_ReturnTracks()
        {
            // Arrange
            var likedTrack1 = new MusicTrack { Title = "ExistTrack3", CreatorId = "Author3" };
            var likedTrack2 = new MusicTrack { Title = "ExistTrack4", CreatorId = "Author3" };
            _musicContext.MusicTracks.AddRange([likedTrack1, likedTrack2]);

            var user = new MusicAuthor { Id = "Author4", Name = "ExistAuthor" };
            user.LikedTracks.AddRange([likedTrack1.Id, likedTrack2.Id]);
            _musicContext.MusicAuthors.Add(user);
            await _musicContext.SaveChangesAsync();

            //Act
            var likedTracks = await _mongoService.GetLikedTracksByAuthorIdAsync(user.Id);

            //Assert
            Assert.NotNull(likedTracks);
            Assert.NotEmpty(likedTracks);
            Assert.Equal(2, likedTracks.Count());

            Assert.Contains(likedTracks, t => t.Title == "ExistTrack3");
            Assert.Contains(likedTracks, t => t.Title == "ExistTrack4");
        }

        [Fact]
        public async Task GetLikedTracksByAuthorIdAsync_NoAuthorExist_ReturnEmptyList()
        {
            // Arrange
            var userId = "testUserId";

            //Act
            var likedTracks = await _mongoService.GetLikedTracksByAuthorIdAsync(userId);

            //Assert
            Assert.NotNull(likedTracks);
            Assert.Empty(likedTracks);
        }

        [Fact]
        public async Task GetLikedTracksByAuthorIdAsync_NoLikedTracks_ReturnEmptyList()
        {
            // Arrange
            var user = new MusicAuthor { Id = "Author5", Name = "ExistAuthor" };
            _musicContext.MusicAuthors.Add(user);
            await _musicContext.SaveChangesAsync();

            //Act
            var likedTracks = await _mongoService.GetLikedTracksByAuthorIdAsync(user.Id);

            //Assert
            Assert.NotNull(likedTracks);
            Assert.Empty(likedTracks);
        }

        [Fact]
        public async Task GetTrackByIdAsync_TrackExists_ReturnsTrack()
        {
            // Arrange
            var track = new MusicTrack { Title = "ExistTrack", CreatorId = "Author3" };
            _musicContext.MusicTracks.Add(track);
            await _musicContext.SaveChangesAsync();

            //Act
            var result = await _mongoService.GetTrackByIdAsync(track.Id.ToString());

            //Assert
            Assert.NotNull(result);
            Assert.Equal(track.Title, result.Title);
            Assert.Equal(track.CreatorId, result.CreatorId);
        }

        [Fact]
        public async Task GetTrackByIdAsync_TrackNotExists_ReturnsNull()
        {
            // Arrange
            var trackId = "546c776b3e23f5f2ebdd3b03";

            //Act
            var result = await _mongoService.GetTrackByIdAsync(trackId);

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
            _musicContext.MusicTracks.RemoveRange(_musicContext.MusicTracks);

            _musicContext.MusicTracks.AddRange([
                new MusicTrack { Title = "ExistTrack5", CreatorId = "Author5" },
                new MusicTrack { Title = "ExistTrack6", CreatorId = "Author5" }
            ]);
            await _musicContext.SaveChangesAsync();

            //Act
            var result = await _mongoService.GetAllTracksAsync();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());

            Assert.Contains(result, t => t.Title == "ExistTrack5");
            Assert.Contains(result, t => t.Title == "ExistTrack6");
        }

        [Fact]
        public async Task GetAllTracksAsync_ListIsNull_ReturnsNull()
        {
            // Arrange
            _musicContext.MusicTracks.RemoveRange(_musicContext.MusicTracks);
            await _musicContext.SaveChangesAsync();

            //Act
            var result = await _mongoService.GetAllTracksAsync();

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
