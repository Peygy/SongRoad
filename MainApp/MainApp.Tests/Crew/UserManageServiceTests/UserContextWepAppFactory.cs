using MainApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MainApp.Tests.Crew.UserManageServiceTests
{
    public class UserContextWepAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreContainer;

        public UserContextWepAppFactory()
        {
            _postgreContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                var descriptor = services
                    .SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<UserContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<UserContext>(options =>
                    options.UseNpgsql(_postgreContainer.GetConnectionString()));
            });
        }

        public async Task InitializeAsync()
        {
            await _postgreContainer.StartAsync();

            await _postgreContainer.ExecScriptAsync("create extension hstore;");
        }

        public new Task DisposeAsync()
        {
            return _postgreContainer.StopAsync();
        }
    }
}
