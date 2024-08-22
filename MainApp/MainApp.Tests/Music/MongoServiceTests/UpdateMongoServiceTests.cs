using MainApp.Models.Music;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MainApp.Tests.Music.MongoServiceTests
{
    public class UpdateMongoServiceTests : BaseMongoServiceTests
    {
        public UpdateMongoServiceTests(MongoWebAppFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task UpdateTrackAsync_TrackExists_ReturnTrue()
        {
            // Arrange
            var style = _musicContext.Styles.First();

            var oldTrack = new MusicTrack
            {
                Title = "ExistTrack1",
                CreatorId = "Author1",
                Style = style,
                TrackImage = new TrackImageModel() { ImageData = [0, 1, 2], ContentType = "image/jpeg" }
            };
            _musicContext.MusicTracks.Add(oldTrack);
            await _musicContext.SaveChangesAsync();

            var updatedTrack = oldTrack;
            updatedTrack.Title = "NewExistTrack1";
            var newStyle = await _musicContext.Styles.FirstOrDefaultAsync(s => s.Name == "rock");
            if (newStyle != null)
            {
                updatedTrack.Style = newStyle;
            }
            updatedTrack.TrackImage = new TrackImageModel() { ImageData = [3, 4, 5], ContentType = "image/jpeg" };

            //Act
            var result = await _mongoService.UpdateTrackAsync(updatedTrack);

            //Assert
            Assert.True(result);

            var track = await _musicContext.MusicTracks.FirstOrDefaultAsync(t => t.Id == oldTrack.Id);
            Assert.NotNull(track);
            Assert.Equal(updatedTrack.Title, track.Title);
        }

        [Fact]
        public async Task UpdateTrackAsync_TrackNotExists_ReturnFalse()
        {
            // Arrange
            var nonExistentTrack = new MusicTrack
            {
                Id = ObjectId.Parse("546c776b3e23f5f2ebdd3b01"),
                Title = "NonExistentTrack",
                CreatorId = "AuthorNotExist"
            };

            //Act
            var result = await _mongoService.UpdateTrackAsync(nonExistentTrack);

            //Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTrackAsync_TrackExistsAndNoModifications_ReturnFalse()
        {
            // Arrange
            var style = _musicContext.Styles.First();

            var oldTrack = new MusicTrack
            {
                Title = "ExistTrack3",
                CreatorId = "Author3",
                Style = style,
                TrackImage = new TrackImageModel() { ImageData = [0, 1, 2], ContentType = "image/jpeg" }
            };
            _musicContext.MusicTracks.Add(oldTrack);
            await _musicContext.SaveChangesAsync();

            //Act
            var result = await _mongoService.UpdateTrackAsync(oldTrack);

            //Assert
            Assert.False(result);

            var track = await _musicContext.MusicTracks.FirstOrDefaultAsync(t => t.Id == oldTrack.Id);
            Assert.NotNull(track);
            Assert.Equal(oldTrack.Title, track.Title);
        }
    }
}
