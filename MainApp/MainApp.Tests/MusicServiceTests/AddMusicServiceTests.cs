﻿using MainApp.DTO.Music;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Http;
using Moq;

namespace MainApp.Tests.MusicServiceTests
{
    public class AddMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task AddTrackAsync_ShouldReturnTrue_WhenTrackIsAddedSuccessfully()
        {
            // Arrange
            var musicTrackModel = new NewMusicTrackModelDTO
            {
                Title = "testTrack",
                Style = "pop"
            };
            var imageModel = new TrackImageModel { ImageData = [1, 2, 3], ContentType = "image/jpeg" };

            _mockMongoService.Setup(m => m.AddMusicTrackImageAsync(It.IsAny<IFormFile>())).ReturnsAsync(imageModel);
            _mockMongoService.Setup(m => m.AddNewTrackAsync(It.IsAny<MusicTrack>(), It.IsAny<string>())).ReturnsAsync("testTrackId");

            // Act
            var result = await _musicService.AddTrackAsync(musicTrackModel, "testUserId");

            // Assert
            Assert.True(result);
            _mockMongoService.Verify(m => m.AddMusicTrackImageAsync(It.IsAny<IFormFile>()), Times.Once);
            _mockMongoService.Verify(m => m.AddNewTrackAsync(It.IsAny<MusicTrack>(), It.IsAny<string>()), Times.Once);
            _mockDriveApi.Verify(d => d.UploadFile(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddTrackAsync_ShouldReturnFalse_WhenTrackNotAdded()
        {
            // Arrange
            var musicTrackModel = new NewMusicTrackModelDTO
            {
                Title = "testTrack",
                Style = "pop"
            };
            var imageModel = new TrackImageModel { ImageData = [1, 2, 3], ContentType = "image/jpeg" };

            _mockMongoService.Setup(m => m.AddMusicTrackImageAsync(It.IsAny<IFormFile>())).ReturnsAsync(imageModel);
            _mockMongoService.Setup(m => m.AddNewTrackAsync(It.IsAny<MusicTrack>(), It.IsAny<string>())).ReturnsAsync((string)null);

            // Act
            var result = await _musicService.AddTrackAsync(musicTrackModel, "testUserId");

            // Assert
            Assert.False(result);
            _mockMongoService.Verify(m => m.AddMusicTrackImageAsync(It.IsAny<IFormFile>()), Times.Once);
            _mockMongoService.Verify(m => m.AddNewTrackAsync(It.IsAny<MusicTrack>(), It.IsAny<string>()), Times.Once);
            _mockDriveApi.Verify(d => d.UploadFile(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddLikedTrackAsync_ShouldCallAddLikedUserTrackAsync()
        {
            // Arrange
            var trackId = "testTrackId";
            var userId = "testUserId";

            // Act
            await _musicService.AddLikedTrackAsync(trackId, userId);

            // Assert
            _mockMongoService.Verify(m => m.AddLikedUserTrackAsync(trackId, userId), Times.Once);
        }
    }
}
