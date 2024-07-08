using MainApp.Data;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace MainApp.Tests
{
    public class MongoDbFactory : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer;
        public IMongoDatabase Database { get; private set; }
        public MusicContext Context { get; private set; }

        public MongoDbFactory()
        {
            _mongoDbContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .WithPortBinding(27017, true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _mongoDbContainer.StartAsync();
            Context = new MusicContext
            {
                ConnectionURL = _mongoDbContainer.GetConnectionString(),
                DatabaseName = "mongo_test",
                CollectionNames = new List<string> { "tracks", "styles", "tracks_images", "music_authors" }
            };

            var client = new MongoClient(_mongoDbContainer.GetConnectionString());
            Database = client.GetDatabase(Context.DatabaseName);
        }

        public Task DisposeAsync() => _mongoDbContainer.DisposeAsync().AsTask();
    }
}
