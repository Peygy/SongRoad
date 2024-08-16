using MongoDB.Driver;
using MainApp.Models.Music;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Tests.Music.MongoServiceTests
{
    public class AddMongoServiceTests : BaseMongoServiceTests
    {
        public AddMongoServiceTests(MongoWebAppFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task AddNewTrackAsync_TrackDoesNotExist_InsertsTrack()
        {
            // Arrange
            var userId = "Author1";
            var track = new MusicTrack
            {
                Title = "NewTrack",
                CreatorId = "Author1",
                Style = _musicContext.Styles.First(),
                TrackImage = new TrackImageModel() { ImageData = [0, 1, 2], ContentType = "image/jpeg" }
            };
            await _musicContext.SaveChangesAsync();

            // Act
            var style = await _musicContext.Styles.FirstOrDefaultAsync(m => m.Name == "pop");
            Assert.NotNull(style);

            var result_trackId = await _mongoService.AddNewTrackAsync(track, style.Id.ToString());

            // Assert
            Assert.NotNull(result_trackId);

            var addedTrack = await _musicContext.MusicTracks
                .FirstOrDefaultAsync(m => m.Id.ToString() == result_trackId);
            Assert.NotNull(addedTrack);
            Assert.Equal(result_trackId, addedTrack.Id.ToString());
            Assert.Equal(userId, addedTrack.CreatorId);
        }

        [Fact]
        public async Task AddNewTrackAsync_TrackExist_ReturnNull()
        {
            // Arrange
            var track = new MusicTrack { Title = "ExistTrack1", CreatorId = "Author2" };
            _musicContext.MusicTracks.Add(track);
            await _musicContext.SaveChangesAsync();

            // Act
            var style = await _musicContext.Styles.FirstOrDefaultAsync(m => m.Name == "pop");
            Assert.NotNull(style);

            var result_trackId = await _mongoService.AddNewTrackAsync(track, style.Id.ToString());

            // Assert
            Assert.Null(result_trackId);

            var addedTrack = await _musicContext.MusicTracks
                .FirstOrDefaultAsync(m => m.Id.ToString() == result_trackId);
            Assert.Null(addedTrack);
        }

        [Fact]
        public async Task AddLikedUserTrackAsync_TrackIsAdded_ReturnTrue()
        {
            // Arrange
            var user = new MusicAuthor { Id = "Author3", Name = "Author3" };
            _musicContext.MusicAuthors.Add(user);
            var track = new MusicTrack { Title = "ExistTrack2", CreatorId = "Author2" };
            _musicContext.MusicTracks.Add(track);
            await _musicContext.SaveChangesAsync();

            // Act
            var result = await _mongoService.AddLikedUserTrackAsync(track.Id.ToString(), user.Id);

            // Assert
            Assert.True(result);
            Assert.NotEmpty(user.LikedTracks);
            Assert.Contains(track.Id, user.LikedTracks);
        }

        [Fact]
        public async Task AddLikedUserTrackAsync_UserIsNull_ReturnFalse()
        {
            // Arrange
            var userId = "Author4";
            var testTrackId = "testTrackId";

            // Act
            var result = await _mongoService.AddLikedUserTrackAsync(testTrackId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddLikedUserTrackAsync_TrackNotFound_ReturnFalse()
        {
            // Arrange
            var user = new MusicAuthor { Id = "Author5", Name = "Author5" };
            await _musicContext.SaveChangesAsync();
            var testTrackId = "testTrackId";

            // Act
            var result = await _mongoService.AddLikedUserTrackAsync(testTrackId, user.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddLikedUserTrackAsync_TrackAlreadyLiked_ReturnFalse()
        {
            // Arrange
            var track = new MusicTrack { Title = "ExistTrack3", CreatorId = "Author2" };
            _musicContext.MusicTracks.Add(track);
            var user = new MusicAuthor { Id = "Author6", Name = "Author6" };
            user.LikedTracks.Add(track.Id);
            _musicContext.MusicAuthors.Add(user);
            await _musicContext.SaveChangesAsync();

            // Act
            var result = await _mongoService.AddLikedUserTrackAsync(track.Id.ToString(), user.Id);

            // Assert
            Assert.False(result);
        }
    }
}