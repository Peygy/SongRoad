using MainApp.Interfaces.Music;
using MainApp.Models.User;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Authorization;
using MainApp.DTO.Music;

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

        public async Task AddTrackAsync(MusicTrackModelDTO musicTrackModel, string userId)
        {
            // Add to mongo
            var track = new MusicTrack() {
                Title = musicTrackModel.Title,
                CreatorId = userId
            };

            await mongoService.AddNewTrackAsync(track, musicTrackModel.Style);

            // Add to file storage - drive
            await driveApiService.UploadMusicFileToGoogleDrive(musicTrackModel.Mp3File);
        }

        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await mongoService.GetMusicStylesAsync();
        }
    }
}
