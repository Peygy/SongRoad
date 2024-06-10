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
            var imageModel = await mongoService.AddMusicTrackImageAsync(musicTrackModel.TrackImage);

            // Create new model of music track
            var track = new MusicTrack() {
                Title = musicTrackModel.Title,
                TrackImage = imageModel,
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
                musicTracks.Add(CreateMusicTrackModelDTO(musicTrack));
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
        /// Method for get music track model by id
        /// </summary>
        /// <param name="trackId">Music track id</param>
        /// <returns>DTO model of music track</returns>
        public async Task<MusicTrackModelDTO?> GetMusicTrackByIdAsync(string trackId)
        {
            var musicTrackModel = await mongoService.GetTrackByIdAsync(trackId);
            if (musicTrackModel != null)
            {
                return CreateMusicTrackModelDTO(musicTrackModel);
            }

            return null;
        }

        /// <summary>
        /// Method for get list of music track's styles
        /// </summary>
        /// <returns>List of music styles</returns>
        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await mongoService.GetMusicStylesAsync();
        }

        /// <summary>
        /// Method for update music track info (data, image and mp3 file)
        /// </summary>
        /// <param name="trackId">Music track id</param>
        /// <param name="musicTrackModel">Updating music track model</param>
        /// <returns>Task object</returns>
        /// <exception cref="Exception">Music track not found by id</exception>
        public async Task UpdateMusicTrackAsync(string trackId, NewMusicTrackModelDTO musicTrackModel)
        {
            var track = await mongoService.GetTrackByIdAsync(trackId);

            if (track != null)
            {
                var updateImageTask = Task.Run(() => mongoService.UpdateMusicTrackImageAsync(track, musicTrackModel.TrackImage));

                var style = (await mongoService.GetMusicStylesAsync()).FirstOrDefault(s => s.Id == musicTrackModel.Style);

                track.Title = musicTrackModel.Title;
                track.Style = style;
                track.CreationDate = DateTime.Now;

                // Update music data to mongo storage
                await mongoService.UpdateTrackByIdAsync(track);

                if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
                {
                    // Update to file storage - drive
                    _ = Task.Run(() => driveApiService.UpdateMusicFileFromGoogleDrive(musicTrackModel.Mp3File, trackId));
                }

                // Wait while image upload will be completed
                await Task.WhenAll(updateImageTask);
            }
            else
            {
                throw new Exception("Track not found");
            }
        }

        /// <summary>
        /// Method for create DTO model of music track
        /// </summary>
        /// <param name="musicTrack">Music track model</param>
        /// <returns>DTO model of music track</returns>
        private MusicTrackModelDTO CreateMusicTrackModelDTO(MusicTrack musicTrack)
        {
            return new MusicTrackModelDTO()
            {
                Title = musicTrack.Title,
                Style = musicTrack.Style.Name,
                CreationDate = musicTrack.CreationDate.ToString(),
                FileId = musicTrack.Id,
                ImageBase64 = Convert.ToBase64String(musicTrack.TrackImage.ImageData)
            };
        }
    }
}
