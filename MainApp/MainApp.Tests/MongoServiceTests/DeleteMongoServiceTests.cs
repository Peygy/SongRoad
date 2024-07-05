using MainApp.Models.Music;
using MainApp.Services;
using MongoDB.Driver;
using System.Reflection;

namespace MainApp.Tests.MongoServiceTests
{
    public class DeleteMongoServiceTests : BaseIntegrationTest
    {
        public DeleteMongoServiceTests(MongoDbFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task DeleteTrackByIdAsync_TrackExists_ReturnTrue()
        {
            // Arrange
            var musicTrack = new MusicTrack
            {
                Title = "ExistTrack1",
                TrackImage = new TrackImageModel()
                {
                    ImageData = [1, 2, 3]
                }
            };
            await _tracksCollection.InsertOneAsync(musicTrack);

            // Act
            var result = await _mongoService.DeleteTrackByIdAsync(musicTrack.Id);

            // Assert
            Assert.True(result);
            var deletedTrack = await _tracksCollection.Find(m => m.Id == musicTrack.Id).FirstOrDefaultAsync();
            Assert.Null(deletedTrack);
        }

        [Fact]
        public async Task DeleteTrackByIdAsync_TrackNotExists_ReturnFalse()
        {
            //Arrange
            var musicTrack = new MusicTrack();
            await _tracksCollection.InsertOneAsync(musicTrack);
            await _tracksCollection.DeleteOneAsync(m => m.Id == musicTrack.Id);
            Assert.NotNull(musicTrack.Id);

            // Act
            var result = await _mongoService.DeleteTrackByIdAsync(musicTrack.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteImageTrackByIdAsync_ImageExists_Delete()
        {
            // Arrange
            var trackImage = new TrackImageModel()
            {
                ContentType = "image/jpeg",
                ImageData = [1, 2, 3]
            };
            await _tracksImagesCollection.InsertOneAsync(trackImage);

            var mongoService = new MongoService(_mockMongoContext.Object, _mockConfiguration.Object);
            var method = typeof(MongoService).GetMethod("DeleteImageTrackByIdAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { trackImage.Id };

            // Act
            _ = method.Invoke(mongoService, parameters) as Task;

            // Assert
            var deletedTrack = await _tracksImagesCollection.Find(m => m.Id == trackImage.Id).FirstOrDefaultAsync();
            Assert.Null(deletedTrack);
        }

        [Fact]
        public async Task DeleteImageTrackByIdAsync_ImageNotExists_NotDelete()
        {
            // Arrange
            var trackImage = new TrackImageModel()
            {
                ContentType = "image/jpeg",
                ImageData = [1, 2, 3]
            };
            await _tracksImagesCollection.InsertOneAsync(trackImage);
            await _tracksImagesCollection.DeleteOneAsync(i => i.Id == trackImage.Id);

            var mongoService = new MongoService(_mockMongoContext.Object, _mockConfiguration.Object);
            var method = typeof(MongoService).GetMethod("DeleteImageTrackByIdAsync", BindingFlags.NonPublic | BindingFlags.Instance);;

            object[] parameters = { trackImage.Id };

            // Act
            _ = method.Invoke(mongoService, parameters) as Task;

            // Assert
            var deletedTrack = await _tracksImagesCollection.Find(m => m.Id == trackImage.Id).FirstOrDefaultAsync();
            Assert.Null(deletedTrack);
        }

        [Fact]
        public async Task DeleteTrackFromAuthorsAsync_ShouldDeleteTrack_FromUploadedAndLikedLists()
        {
            // Arrange
            var trackImage = new TrackImageModel()
            {
                ContentType = "image/jpeg",
                ImageData = [4, 5, 6]
            };
            await _tracksImagesCollection.InsertOneAsync(trackImage);

            await _musicAuthorsCollection.InsertManyAsync(new List<MusicAuthor> {
                new MusicAuthor() { Id = "1", UploadedTracksId = { trackImage.Id } },
                new MusicAuthor() { Id = "2", LikedTracksId = { trackImage.Id } },
                new MusicAuthor() { Id = "3", LikedTracksId = { trackImage.Id } }
            });

            var mongoService = new MongoService(_mockMongoContext.Object, _mockConfiguration.Object);
            var method = typeof(MongoService).GetMethod("DeleteTrackFromAuthorsAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { "1", trackImage.Id };

            // Act
            _ = method.Invoke(mongoService, parameters) as Task;

            // Assert
            var author = await _musicAuthorsCollection.Find(a => a.Id == "1").FirstOrDefaultAsync();
            Assert.Empty(author.UploadedTracksId);
            Assert.DoesNotContain(trackImage.Id, author.UploadedTracksId);

            author = await _musicAuthorsCollection.Find(a => a.Id == "2").FirstOrDefaultAsync();
            Assert.Empty(author.LikedTracksId);
            Assert.DoesNotContain(trackImage.Id, author.LikedTracksId);

            author = await _musicAuthorsCollection.Find(a => a.Id == "3").FirstOrDefaultAsync();
            Assert.Empty(author.LikedTracksId);
            Assert.DoesNotContain(trackImage.Id, author.LikedTracksId);
        }
    }
}
