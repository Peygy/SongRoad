using MainApp.Models.Service;

namespace MainApp.Services
{
    public class MongoService : IMongoService
    {
        private readonly IConfiguration configuration;

        public MongoService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
