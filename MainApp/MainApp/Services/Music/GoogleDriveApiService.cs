using MainApp.Models.Service;

namespace MainApp.Services
{
    public class GoogleDriveApiService : IGoogleDriveApiService
    {
        private readonly IConfiguration configuration;

        public GoogleDriveApiService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
