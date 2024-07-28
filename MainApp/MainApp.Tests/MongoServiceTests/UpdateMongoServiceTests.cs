using MainApp.Models.Music;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace MainApp.Tests.MongoServiceTests
{
    public class UpdateMongoServiceTests : BaseMongoServiceTests
    {
        public UpdateMongoServiceTests(WebAppFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task UpdateTrackByIdAsync_TrackExists_ReturnTrue()
        {
            // Arrange
            var style = _musicContext.Styles.First();

            var oldTrack = new MusicTrack {
                Title = "ExistTrack1",
                CreatorId = "Author1",
                Style = style,
                TrackImage = new TrackImageModel() { ImageData = [0, 1, 2], ContentType = "image/jpeg" }
            };
            _musicContext.MusicTracks.Add(oldTrack);
            await _musicContext.SaveChangesAsync();

            var updatedTrack = oldTrack;
            updatedTrack.Title = "NewExistTrack2";
            var newStyle = await _musicContext.Styles.FirstOrDefaultAsync(s => s.Name == "rock");
            if (newStyle != null)
            {
                updatedTrack.Style = newStyle;
            }
            updatedTrack.TrackImage = new TrackImageModel() { ImageData = [3, 4, 5], ContentType = "image/jpeg" };

            //Act
            var result = await _mongoService.UpdateTrackByIdAsync(updatedTrack);

            //Assert
            Assert.True(result);

            var track = await _musicContext.MusicTracks.FirstOrDefaultAsync(t => t.Id == oldTrack.Id);
            Assert.NotNull(track);
            Assert.Equal(updatedTrack.Title, track.Title);
        }

        [Fact]
        public async Task UpdateTrackByIdAsync_TrackNotExists_ReturnFalse()
        {
            // Arrange
            var updatedTrack = new MusicTrack { Title = "NewExistTrack2", CreatorId = "Author1" };

            //Act
            var result = await _mongoService.UpdateTrackByIdAsync(updatedTrack);

            //Assert
            Assert.False(result);
        }
    }
}
