using MainApp.Models;
using MainApp.Models.Service;
using Microsoft.AspNetCore.Authorization;

namespace MainApp.Services
{
    [Authorize(Roles = UserRoles.User)]
    public class MusicService : IMusicService
    {
        private readonly IMongoService mongoService;
        private readonly IGoogleDriveApiService driveApiService;

        public MusicService(IMongoService mongoService, IGoogleDriveApiService driveApiService)
        {
            this.mongoService = mongoService;
            this.driveApiService = driveApiService;
        }

        public async Task AddUserTrack(string title, string style, IFormFile mp3File, string userId)
        {

        }
    }
}
