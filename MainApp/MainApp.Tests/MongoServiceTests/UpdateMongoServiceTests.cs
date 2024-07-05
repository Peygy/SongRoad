using MainApp.Models.Music;
using MainApp.Services.Music;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Moq;

namespace MainApp.Tests.MongoServiceTests
{
    public class UpdateMongoServiceTests : BaseIntegrationTest
    {
        public UpdateMongoServiceTests(MongoDbFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task UpdateTrackByIdAsync_TrackExists()
        {
            // Arrange
            var oldTrack = new MusicTrack { Title = "ExistTrack1", CreatorId = "Author1" };
            await _tracksCollection.InsertOneAsync(oldTrack);
            var newTrack = new MusicTrack { Id = oldTrack.Id, Title = "NewExistTrack", CreatorId = "Author1" };

            //Act
            await _mongoService.UpdateTrackByIdAsync(newTrack);

            //Assert
            var track = await _tracksCollection.Find(t => t.Id == newTrack.Id).FirstOrDefaultAsync();
            Assert.NotNull(track);
            Assert.NotEqual(oldTrack.Title, track.Title);
            Assert.Equal(newTrack.Title, track.Title);
        }

        [Fact]
        public async Task UpdateTrackByIdAsync_TrackNotExists_ThrowsException()
        {
            // Arrange
            var newTrack = new MusicTrack { Title = "NoExistTrack", CreatorId = "Author2" };

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
                await _mongoService.UpdateTrackByIdAsync(newTrack));

            // Assert
            Assert.Equal("Track not found", exception.Message);
        }

        [Fact]
        public async Task UpdateMusicTrackImageAsync_FileAndRowExist_DiffrentFile_SuccessfullyUpdatesImage()
        {
            // Arrange
            // For old file in db
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.jpg");
            var fileInfo = new FileInfo(filePath);

            var oldImageFileMock = new Mock<IFormFile>();
            using (var oldStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                oldImageFileMock.Setup(_ => _.OpenReadStream()).Returns(oldStream);
                oldImageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
                oldImageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
                oldImageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

                var initialImage = await CompressService.CompressImageFileAsync(oldImageFileMock.Object);
                await _tracksImagesCollection.InsertOneAsync(initialImage);
                var oldImage = await _tracksImagesCollection.Find(i => i == initialImage).FirstOrDefaultAsync();

                var musicTrack = new MusicTrack
                {
                    Title = "ExistTrack2",
                    TrackImage = initialImage
                };

                // For new file from client
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "newTest.jpg");
                fileInfo = new FileInfo(filePath);

                var newImageFileMock = new Mock<IFormFile>();
                using (var newStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    newImageFileMock.Setup(_ => _.OpenReadStream()).Returns(newStream);
                    newImageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
                    newImageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
                    newImageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

                    // Act
                    await _mongoService.UpdateMusicTrackImageAsync(musicTrack, newImageFileMock.Object);
                }

                // Assert
                var updatedImage = await _tracksImagesCollection.Find(i => i.Id == oldImage.Id).FirstOrDefaultAsync();
                Assert.NotNull(updatedImage);
                Assert.NotEqual(oldImage.ImageData, updatedImage.ImageData);
            }
        }

        [Fact]
        public async Task UpdateMusicTrackImageAsync_FileAndRowExist_SameFile()
        {
            // Arrange
            // For old file in db
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.jpg");
            var fileInfo = new FileInfo(filePath);

            var sameImageFileMock = new Mock<IFormFile>();
            using (var oldStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                sameImageFileMock.Setup(_ => _.OpenReadStream()).Returns(oldStream);
                sameImageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
                sameImageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
                sameImageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

                var initialImage = await CompressService.CompressImageFileAsync(sameImageFileMock.Object);
                await _tracksImagesCollection.InsertOneAsync(initialImage);
                var oldImage = await _tracksImagesCollection.Find(i => i == initialImage).FirstOrDefaultAsync();

                var musicTrack = new MusicTrack
                {
                    Title = "ExistTrack3",
                    TrackImage = initialImage
                };

                // For same file from client
                var newImageFileMock = new Mock<IFormFile>();
                using (var newStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    newImageFileMock.Setup(_ => _.OpenReadStream()).Returns(newStream);
                    newImageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
                    newImageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
                    newImageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

                    // Act
                    await _mongoService.UpdateMusicTrackImageAsync(musicTrack, newImageFileMock.Object);
                }

                // Assert
                var notUpdatedImage = await _tracksImagesCollection.Find(i => i.Id == oldImage.Id).FirstOrDefaultAsync();
                Assert.NotNull(notUpdatedImage);
                Assert.Equal(oldImage.ImageData, notUpdatedImage.ImageData);
            }
        }

        [Fact]
        public async Task UpdateMusicTrackImageAsync_FileIsNull_NoUpdate()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.jpg");
            var fileInfo = new FileInfo(filePath);

            var sameImageFileMock = new Mock<IFormFile>();
            using (var oldStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                sameImageFileMock.Setup(_ => _.OpenReadStream()).Returns(oldStream);
                sameImageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
                sameImageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
                sameImageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

                var initialImage = new TrackImageModel()
                {
                    ImageData = [1, 2, 3]
                };
                await _tracksImagesCollection.InsertOneAsync(initialImage);
                var oldImage = await _tracksImagesCollection.Find(i => i == initialImage).FirstOrDefaultAsync();

                var musicTrack = new MusicTrack
                {
                    Title = "ExistTrack4",
                    TrackImage = initialImage
                };

                // Act
                await _mongoService.UpdateMusicTrackImageAsync(musicTrack, null!);

                // Assert
                var notUpdatedImage = await _tracksImagesCollection.Find(i => i.Id == oldImage.Id).FirstOrDefaultAsync();
                Assert.NotNull(notUpdatedImage);
                Assert.Equal(oldImage.ImageData, notUpdatedImage.ImageData);
            }
        }

        [Fact]
        public async Task UpdateMusicTrackImageAsync_RowIsNull_ThrowsException()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.jpg");
            var fileInfo = new FileInfo(filePath);

            var sameImageFileMock = new Mock<IFormFile>();
            using (var oldStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                sameImageFileMock.Setup(_ => _.OpenReadStream()).Returns(oldStream);
                sameImageFileMock.Setup(_ => _.FileName).Returns(fileInfo.Name);
                sameImageFileMock.Setup(_ => _.Length).Returns(fileInfo.Length);
                sameImageFileMock.Setup(_ => _.ContentType).Returns("image/jpeg");

                var musicTrack = new MusicTrack
                {
                    Title = "ExistTrack5",
                    TrackImage = new TrackImageModel()
                    {
                        ImageData = [1, 2, 3]
                    }
                };

                // Act
                var exception = await Assert.ThrowsAsync<Exception>(async () =>
                    await _mongoService.UpdateMusicTrackImageAsync(musicTrack, sameImageFileMock.Object));

                // Assert
                Assert.Equal("Update image failed", exception.Message);
            }
        }
    }
}
