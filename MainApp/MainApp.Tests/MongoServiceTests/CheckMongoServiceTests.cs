using MongoDB.Driver;
using MainApp.Models.Music;
using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Tests.MongoServiceTests
{
    public class CheckMongoServiceTests : BaseMongoServiceTests
    {
        public CheckMongoServiceTests(WebAppFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task Check_AuthorDoesNotExist_InsertsAuthor()
        {
            // Arrange
            var user = new UserModel { Id = "NewAuthor", UserName = "NewAuthor" };

            // Act
            await _mongoService.CheckAuthorExistAsync(user);

            // Assert
            var author = await _musicContext.MusicAuthors.FirstOrDefaultAsync(a => a.Id == user.Id);
            Assert.NotNull(author);
            Assert.Equal(user.Id, author.Id);
            Assert.Equal(user.UserName, author.Name);
        }

        [Fact]
        public async Task Check_AuthorExist()
        {
            // Arrange
            var user = new UserModel { Id = "NewAuthor", UserName = "NewAuthor" };

            // Act
            await _mongoService.CheckAuthorExistAsync(user);

            // Assert
            var authors = _musicContext.MusicAuthors.ToList();
            Assert.Single(authors);
            Assert.Equal(user.Id, authors.First().Id);
            Assert.Equal(user.UserName, authors.First().Name);
        }
    }
}
