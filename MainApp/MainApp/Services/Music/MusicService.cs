using MainApp.Interfaces.Music;
using MainApp.Models;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Authorization;

namespace MainApp.Services
{
    [Authorize(Roles = UserRoles.User)]
    public class MusicService : IMusicService
    {
        private readonly MongoService mongoService;
        private readonly GoogleDriveApiService driveApiService;

        public MusicService(MongoService mongoService, GoogleDriveApiService driveApiService)
        {
            this.mongoService = mongoService;
            this.driveApiService = driveApiService;
        }

        public async Task AddTrackAsync(string title, string style, IFormFile mp3File, string userId)
        {
            // Add to mongo
            var track = new MusicTrack() {
                Title = title,
                CreatorId = userId
            };

            await mongoService.AddNewTrackAsync(track, style);

            // Add to file storage - drive
            await driveApiService.AddMusicFileToGoogleDrive(mp3File);
        }
    }
}
