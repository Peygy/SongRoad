using MongoDB.Driver;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Http;
using Moq;
using MainApp.Models.User;

namespace MainApp.Tests.MongoServiceTests
{
    public class AddMongoServiceTests : BaseMongoServiceTests
    {
        public AddMongoServiceTests(WebAppFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task AddMusicTrackImageAsync_ShouldCompressAndSaveImage()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.jpg");
            var fileInfo = new FileInfo(filePath);

            var imageFileMock = new Mock<IFormFile>();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            imageFileMock.Setup(_ => _.OpenReadStream()).Returns(stream);
            imageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
            imageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
            imageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

            // Act
            var result = await _mongoService.AddMusicTrackImageAsync(imageFileMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TrackImageModel>(result);
            Assert.Equal("image/jpeg", result.ContentType);
            Assert.NotEmpty(result.ImageData);

            var insertedImage = await _tracksImagesCollection.Find(i => i.ImageData == result.ImageData).FirstOrDefaultAsync();
            Assert.NotNull(insertedImage);
        }

        [Fact]
        public async Task AddNewTrackAsync_TrackDoesNotExist_InsertsTrack()
        {
            // Arrange
            var user = new UserModel { Id = "Author1", UserName = "Author1" };
            await _musicAuthorsCollection.InsertOneAsync(new MusicAuthor { Id = user.Id, Name = user.UserName });
            var track = new MusicTrack { Title = "NewTrack", CreatorId = user.Id };

            // Act
            var style = await _stylesCollection.Find(m => m.Name == "pop").FirstOrDefaultAsync();
            Assert.NotNull(style);

            var result_trackId = await _mongoService.AddNewTrackAsync(track, style.Id);

            // Assert
            Assert.NotNull(result_trackId);

            var addedTrack = await _tracksCollection.Find(m => m.Id == result_trackId).FirstOrDefaultAsync();
            Assert.NotNull(addedTrack);
            Assert.Equal(result_trackId, addedTrack.Id);
            Assert.Equal(track.CreatorId, addedTrack.CreatorId);

            var author = await _musicAuthorsCollection.Find(a => a.Id == user.Id).FirstOrDefaultAsync();
            Assert.NotNull(author);
            Assert.NotEmpty(author.UploadedTracksId);
            Assert.Contains(result_trackId, author.UploadedTracksId);
        }

        [Fact]
        public async Task AddNewTrackAsync_TrackExist_ReturnNull()
        {
            // Arrange
            var track = new MusicTrack { Title = "ExistTrack", CreatorId = "Author2" };
            await _tracksCollection.InsertOneAsync(track);

            // Act
            var style = await _stylesCollection.Find(m => m.Name == "pop").FirstOrDefaultAsync();
            Assert.NotNull(style);

            var result_trackId = await _mongoService.AddNewTrackAsync(track, style.Id);

            // Assert
            Assert.Null(result_trackId);

            var addedTrack = await _tracksCollection.Find(m => m.Id == result_trackId).FirstOrDefaultAsync();
            Assert.Null(addedTrack);
        }

        [Fact]
        public async Task AddLikedUserTrackAsync_TrackIsAdded_Success()
        {
            // Arrange
            var user = new UserModel { Id = "Author3", UserName = "Author3" };
            await _musicAuthorsCollection.InsertOneAsync(new MusicAuthor { Id = user.Id, Name = user.UserName });
            var trackId = "TrackId_1";

            // Act
            await _mongoService.AddLikedUserTrackAsync(trackId, user.Id);

            // Assert
            var author = await _musicAuthorsCollection.Find(a => a.Id == user.Id).FirstOrDefaultAsync();
            Assert.NotNull(author);
            Assert.Equal(author.Name, user.UserName);
            Assert.NotEmpty(author.LikedTracksId);
            Assert.Contains(trackId, author.LikedTracksId);
        }

        [Fact]
        public async Task AddLikedUserTrackAsync_UserNotFound_ThrowsException()
        {
            // Arrange
            var user = new UserModel { Id = "Author4", UserName = "Author4" };
            var trackId = "TrackId_2";

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _mongoService.AddLikedUserTrackAsync(trackId, user.Id));

            // Assert
            Assert.Equal("User liked tracks not found", exception.Message);
        }
    }
}