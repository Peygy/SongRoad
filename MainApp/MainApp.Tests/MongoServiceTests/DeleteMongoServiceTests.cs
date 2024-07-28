using MainApp.Models.Music;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace MainApp.Tests.MongoServiceTests
{
    public class DeleteMongoServiceTests : BaseMongoServiceTests
    {
        public DeleteMongoServiceTests(WebAppFactory dbFactory) : base(dbFactory) { }
        
        [Fact]
        public async Task DeleteTrackByIdAsync_TrackExists_ReturnTrue()
        {
            // Arrange
            var musicTrack = new MusicTrack { Title = "ExistTrack1", CreatorId = "Author1" };
            _musicContext.MusicTracks.Add(musicTrack);
            await _musicContext.SaveChangesAsync();

            // Act
            var result = await _mongoService.DeleteTrackByIdAsync(musicTrack.Id.ToString());

            // Assert
            Assert.True(result);
            var deletedTrack = await _musicContext.MusicTracks.FirstOrDefaultAsync(m => m.Id == musicTrack.Id);
            Assert.Null(deletedTrack);
        }

        [Fact]
        public async Task DeleteTrackByIdAsync_TrackNotExists_ReturnFalse()
        {
            //Arrange
            var musicTrackId = "546c776b3e23f5f2ebdd3b01";

            // Act
            var result = await _mongoService.DeleteTrackByIdAsync(musicTrackId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTrackFromLikedTracksAsync_ShouldDeleteTrack_ReturnTrue()
        {
            // Arrange
            var track = new MusicTrack { Title = "ExistTrack2", CreatorId = "Author2" };
            _musicContext.MusicTracks.Add(track);
            var user = new MusicAuthor { Id = "Author1", Name = "Author1" };
            user.LikedTracks.Add(track.Id);
            _musicContext.MusicAuthors.Add(user);
            await _musicContext.SaveChangesAsync();

            // Act
            var result = await _mongoService.DeleteTrackFromLikedTracksAsync(user.Id, track.Id.ToString());

            // Assert
            Assert.True(result);
            var userDB = await _musicContext.MusicAuthors.FirstOrDefaultAsync(m => m.Id == user.Id);
            Assert.NotNull(userDB);
            Assert.Empty(userDB.LikedTracks);
        }

        [Fact]
        public async Task DeleteTrackFromLikedTracksAsync_AuthorIsNull_ReturnFalse()
        {
            // Arrange
            var userId = "userId";
            var musicTrackId = "546c776b3e23f5f2ebdd3b02";

            // Act
            var result = await _mongoService.DeleteTrackFromLikedTracksAsync(userId, musicTrackId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTrackFromLikedTracksAsync_TrackNotExist_ReturnTrue()
        {
            // Arrange
            var user = new MusicAuthor { Id = "Author2", Name = "Author2" };
            _musicContext.MusicAuthors.Add(user);
            await _musicContext.SaveChangesAsync();
            var musicTrackId = "546c776b3e23f5f2ebdd3b03";

            // Act
            var result = await _mongoService.DeleteTrackFromLikedTracksAsync(user.Id, musicTrackId);

            // Assert
            Assert.True(result);
        }
    }
}
