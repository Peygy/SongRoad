using MainApp.Models.Music;
using MainApp.Services;
using MongoDB.Driver;
using System.Reflection;

namespace MainApp.Tests.MongoServiceTests
{
    public class InitMongoServiceTests : BaseIntegrationTest
    {
        public InitMongoServiceTests(MongoDbFactory dbFactory) : base(dbFactory) { }

        [Fact]
        public async Task InitMusicStylesCollectionAsync_StylesExist_NotInitStyles()
        {
            // Arrange
            await _stylesCollection.DeleteManyAsync(Builders<Style>.Filter.Empty);
            // Init exist styles
            var styles = new string[] { "testStyle1", "testStyle2", "testStyle3" };
            var styleList = new List<Style>();
            foreach (var styleName in styles)
            {
                var style = new Style
                {
                    Name = styleName,
                };

                styleList.Add(style);
            }
            await _stylesCollection.InsertManyAsync(styleList, new InsertManyOptions { IsOrdered = false });

            var mongoService = new MongoService(_mockMongoContext.Object, _mockConfiguration.Object);
            var method = typeof(MongoService).GetMethod("InitMusicStylesCollectionAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { new string[] { "testStyle" } };

            // Act
            _ = method.Invoke(mongoService, parameters) as Task;

            // Assert
            var stylesCollections = await _stylesCollection.Find(_ => true).ToListAsync();
            Assert.NotEmpty(stylesCollections);
            Assert.NotEqual(1, stylesCollections.Count);
            Assert.Equal(3, stylesCollections.Count);
        }

        [Fact]
        public async Task InitMusicStylesCollectionAsync_StylesNotExist_InitStyles()
        {
            // Arrange
            await _stylesCollection.DeleteManyAsync(Builders<Style>.Filter.Empty);
            var styles = new string[] { "testStyle4", "testStyle5", "testStyle6", "testStyle7" };

            var mongoService = new MongoService(_mockMongoContext.Object, _mockConfiguration.Object);
            var method = typeof(MongoService).GetMethod("InitMusicStylesCollectionAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            object[] parameters = { styles };

            // Act
            _ = method.Invoke(mongoService, parameters) as Task;

            // Assert
            var stylesCollections = await _stylesCollection.Find(_ => true).ToListAsync();
            Assert.NotEmpty(stylesCollections);
            Assert.Equal(4, stylesCollections.Count);
        }
    }
}
