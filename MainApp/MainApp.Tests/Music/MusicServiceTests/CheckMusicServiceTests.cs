using MainApp.Models.User;
using Moq;

namespace MainApp.Tests.Music.MusicServiceTests
{
    public class CheckMusicServiceTests : BaseMusicServiceTests
    {
        [Fact]
        public async Task CheckAuthorExistAsync_ShouldCallCheckAuthorExistAsync()
        {
            // Arrange
            var user = new UserModel { Id = "NewAuthor", UserName = "NewAuthor" };

            // Act
            await _musicService.CheckAuthorExistAsync(user);

            // Assert
            _mockMongoService.Verify(m => m.CheckAuthorExistAsync(user), Times.Once);
        }
    }
}
