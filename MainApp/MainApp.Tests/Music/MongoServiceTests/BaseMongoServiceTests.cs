using MainApp.Data;
using MainApp.Services.Music;
using Microsoft.Extensions.DependencyInjection;

namespace MainApp.Tests.Music.MongoServiceTests
{
    public abstract class BaseMongoServiceTests : IClassFixture<MongoWebAppFactory>
    {
        protected readonly IMongoService _mongoService;
        protected readonly MusicContext _musicContext;

        protected BaseMongoServiceTests(MongoWebAppFactory factory)
        {
            var _scope = factory.Services.CreateScope();

            _mongoService = _scope.ServiceProvider.GetRequiredService<IMongoService>();
            _musicContext = _scope.ServiceProvider.GetRequiredService<MusicContext>();
        }
    }
}
