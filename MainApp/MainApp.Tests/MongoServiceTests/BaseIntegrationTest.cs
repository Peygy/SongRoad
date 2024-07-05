using MainApp.Data;
using MainApp.Models.Music;
using MainApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;

namespace MainApp.Tests.MongoServiceTests
{
    public class BaseIntegrationTest : IClassFixture<MongoDbFactory>
    {
        protected readonly IMongoCollection<MusicTrack> _tracksCollection;
        protected readonly IMongoCollection<Style> _stylesCollection;
        protected readonly IMongoCollection<TrackImageModel> _tracksImagesCollection;
        protected readonly IMongoCollection<MusicAuthor> _musicAuthorsCollection;
        protected readonly MongoService _mongoService;

        protected readonly Mock<IConfiguration> _mockConfiguration;
        protected readonly Mock<IOptions<MusicContext>> _mockMongoContext;

        public BaseIntegrationTest(MongoDbFactory dbFactory)
        {
            _tracksCollection = dbFactory.Database.GetCollection<MusicTrack>("tracks");
            _stylesCollection = dbFactory.Database.GetCollection<Style>("styles");
            _tracksImagesCollection = dbFactory.Database.GetCollection<TrackImageModel>("tracks_images");
            _musicAuthorsCollection = dbFactory.Database.GetCollection<MusicAuthor>("music_authors");

            var _stylesSectionMock = new Mock<IConfigurationSection>();
            var styles = new List<IConfigurationSection>
            {
                CreateConfigurationSection("rock"),
                CreateConfigurationSection("pop"),
                CreateConfigurationSection("rap"),
                CreateConfigurationSection("electronic")
            };
            _stylesSectionMock.Setup(x => x.GetChildren()).Returns(styles);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x.GetSection("Music:Styles")).Returns(_stylesSectionMock.Object);

            _mockMongoContext = new Mock<IOptions<MusicContext>>();
            _mockMongoContext.Setup(m => m.Value).Returns(dbFactory.Context);

            _mongoService = new MongoService(_mockMongoContext.Object, _mockConfiguration.Object);
        }

        private static IConfigurationSection CreateConfigurationSection(string value)
        {
            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns(value);
            return mockSection.Object;
        }
    }
}
