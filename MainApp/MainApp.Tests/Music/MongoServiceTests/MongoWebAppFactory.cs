using MainApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;

namespace MainApp.Tests.Music.MongoServiceTests
{
    public class MongoWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer;

        public MongoWebAppFactory()
        {
            _mongoDbContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .WithPortBinding(27017, true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services
                    .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<MusicContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<MusicContext>(options =>
                    options.UseMongoDB(_mongoDbContainer.GetConnectionString(), "mongo_test"));
            });
        }

        public Task InitializeAsync()
        {
            return _mongoDbContainer.StartAsync();
        }

        public new Task DisposeAsync()
        {
            return _mongoDbContainer.StopAsync();
        }
    }
}
