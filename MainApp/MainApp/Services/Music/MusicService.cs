using MainApp.Interfaces.Music;
using MainApp.Models.User;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Authorization;
using MainApp.DTO.Music;

namespace MainApp.Services
{
    /// <summary>
    /// Class of service for actions with music tracks
    /// </summary>
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

        /// <summary>
        /// Method for add new music track
        /// </summary>
        /// <param name="musicTrackModel">New music track DTO model</param>
        /// <param name="userId">Current user id</param>
        /// <returns>Task object</returns>
        public async Task AddTrackAsync(NewMusicTrackModelDTO musicTrackModel, string userId)
        {
            // Create new model of music track
            var track = new MusicTrack() {
                Title = musicTrackModel.Title,
                CreatorId = userId
            };

            // Add music data to mongo storage
            var trackId = await mongoService.AddNewTrackAsync(track, musicTrackModel.Style);

            if (trackId != null)
            {
                // Add to file storage - drive
                await driveApiService.UploadMusicFileToGoogleDrive(musicTrackModel.Mp3File, trackId);
            }
        }

        /// <summary>
        /// Method for get list of user's personal (where he's creator) files on google drive - cloud storage
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of DTO music tracks models</returns>
        public async Task<List<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId)
        {
            var musicTracksData = (await mongoService.GetAllTracksAsync()).Where(m => m.CreatorId == userId).ToList();

            var musicTracks = new List<MusicTrackModelDTO>();
            foreach (var musicTrack in musicTracksData)
            {
                musicTracks.Add(new MusicTrackModelDTO()
                {
                    Title = musicTrack.Title,
                    Style = musicTrack.Style.Name,
                    FileId = musicTrack.Id
                });
            }

            return musicTracks;
        }

        /// <summary>
        /// Method for get stream of file on google drive - cloud storage
        /// </summary>
        /// <param name="trackId">Music track ID in storage</param>
        /// <returns>File stream</returns>
        public async Task<Stream> GetMusicTrackStreamAsync(string trackId)
        {
            return await driveApiService.DownloadMusicFileFromGoogleDrive(trackId);
        }

        /// <summary>
        /// Method for get list of music track's styles
        /// </summary>
        /// <returns>List of music styles</returns>
        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await mongoService.GetMusicStylesAsync();
        }
    }
}
