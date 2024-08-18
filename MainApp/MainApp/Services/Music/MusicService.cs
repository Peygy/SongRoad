using MainApp.Models.User;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Authorization;
using MainApp.DTO.Music;

namespace MainApp.Services.Music
{
    /// <summary>
    /// Defines the contract for a service that interacts with music data.
    /// </summary>
    public interface IMusicService
    {
        /// <summary>
        /// Checks if the <paramref name="user"/> exists as an author.
        /// </summary>
        /// <param name="user">User who made the request.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task CheckAuthorExistAsync(UserModel? user);

        /// <summary>
        /// Adds a new music track with <paramref name="musicTrackModel"/> track data
        /// by user, who has specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="musicTrackModel">The music track data to add.</param>
        /// <param name="userId">The identifier of the user who is adding the track.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the operation was successful.
        /// </returns>
        Task<bool> AddTrackAsync(NewMusicTrackModelDTO musicTrackModel, string userId);
        /// <summary>
        /// Adds a track, which has specified <paramref name="trackId"/>, 
        /// to the liked tracks of a user, who has specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to be liked.</param>
        /// <param name="userId">The identifier of the user who liked the track.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the operation was successful.
        /// </returns>
        Task<bool> AddLikedTrackAsync(string trackId, string userId);

        /// <summary>
        /// Gets a music track by its identifier <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing the <see cref="MusicTrackModelDTO"/> if found, or null if not.
        /// </returns>
        Task<MusicTrackModelDTO?> GetMusicTrackByIdAsync(string trackId);
        /// <summary>
        /// Gets all music tracks, optionally filtered "liked" by user identifier <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The identifier of the user to add "liked" filter for tracks, 
        /// or null to get all tracks without any filter.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="MusicTrackModelDTO"/> objects.
        /// </returns>
        Task<IEnumerable<MusicTrackModelDTO>> GetAllMusicTracksAsync(string? userId = null);
        /// <summary>
        /// Gets the uploaded tracks of a specific user by his <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="MusicTrackModelDTO"/> objects.
        /// </returns>
        Task<IEnumerable<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId);
        /// <summary>
        /// Gets the liked tracks of a specific user by his <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="MusicTrackModelDTO"/> objects.
        /// </returns>
        Task<IEnumerable<MusicTrackModelDTO>> GetAllLikedMusicTracksAsync(string userId);
        /// <summary>
        /// Gets all available music styles.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="Style"/> objects.
        /// </returns>
        Task<IEnumerable<Style>> GetMusicStylesAsync();

        /// <summary>
        /// Updates a music track with <paramref name="musicTrackModel"/> track data 
        /// by its identifier <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to be updated.</param>
        /// <param name="musicTrackModel">The updated music track data.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the update was successful.
        /// </returns>
        Task<bool> UpdateMusicTrackAsync(string trackId, NewMusicTrackModelDTO musicTrackModel);

        /// <summary>
        /// Deletes a music track by its identifier <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to be deleted.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the deletion was successful.
        /// </returns>
        Task<bool> DeleteMusicTrackAsync(string trackId);
        /// <summary>
        /// Deletes a track, which has specific identifier <paramref name="trackId"/>, 
        /// from a user's, who has specific identifier <paramref name="userId"/>, liked tracks.
        /// </summary>
        /// <param name="userId">The identifier of the user.</param>
        /// <param name="trackId">The identifier of the track to be removed.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the removal was successful.
        /// </returns>
        Task<bool> DeleteLikedTrackAsync(string userId, string trackId);
    }

    [Authorize(Roles = UserRoles.User)]
    public class MusicService : IMusicService
    {
        /// <summary>
        /// The service for interacting with the MongoDB database.
        /// </summary>
        private readonly IMongoService mongoService;
        /// <summary>
        /// The service for interacting with Google Drive microservice.
        /// </summary>
        private readonly IGoogleDriveAppConnectorService driveAppConnectorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicService"/> class.
        /// </summary>
        /// <param name="mongoService">The service for interacting with the MongoDB database.</param>
        /// <param name="driveAppConnectorService">The service for interacting with Google Drive microservice.</param>
        public MusicService(IMongoService mongoService, IGoogleDriveAppConnectorService driveAppConnectorService)
        {
            this.mongoService = mongoService;
            this.driveAppConnectorService = driveAppConnectorService;
        }

        public async Task CheckAuthorExistAsync(UserModel? user)
        {
            await mongoService.CheckAuthorExistAsync(user);
        }

        public async Task<bool> AddTrackAsync(NewMusicTrackModelDTO musicTrackModel, string userId)
        {
            // Create new model of music track
            var track = new MusicTrack() {
                Title = musicTrackModel.Title,
                CreatorId = userId
            };

            if (musicTrackModel.TrackImage != null && musicTrackModel.TrackImage.Length > 0)
            {
                track.TrackImage = await CompressService.CompressImageFileAsync(musicTrackModel.TrackImage);
            }

            var trackId = await mongoService.AddNewTrackAsync(track, musicTrackModel.StyleId);

            if (trackId != null)
            {
                // Add to music file storage - google drive
                await driveAppConnectorService.UploadFile(musicTrackModel.Mp3File, trackId);
                return true;
            }

            return false;
        }

        public async Task<bool> AddLikedTrackAsync(string trackId, string userId)
        {
            return await mongoService.AddLikedUserTrackAsync(trackId, userId);
        }

        public async Task<MusicTrackModelDTO?> GetMusicTrackByIdAsync(string trackId)
        {
            var musicTrackModel = await mongoService.GetTrackByIdAsync(trackId);
            if (musicTrackModel != null)
            {
                return new MusicTrackModelDTO(musicTrackModel);
            }

            return null;
        }

        public async Task<IEnumerable<MusicTrackModelDTO>> GetAllMusicTracksAsync(string? userId)
        {
            var musicTracks = await mongoService.GetAllTracksAsync();

            return await CreateMusicDTOCollection(musicTracks, userId);
        }

        public async Task<IEnumerable<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId)
        {
            var musicTracks = await mongoService.GetUploadedTracksByAuthorIdAsync(userId);

            return await CreateMusicDTOCollection(musicTracks, userId);
        }

        public async Task<IEnumerable<MusicTrackModelDTO>> GetAllLikedMusicTracksAsync(string userId)
        {
            var musicTracks = await mongoService.GetLikedTracksByAuthorIdAsync(userId);

            return await CreateMusicDTOCollection(musicTracks, userId);
        }

        public async Task<IEnumerable<Style>> GetMusicStylesAsync()
        {
            return await mongoService.GetMusicStylesAsync();
        }

        public async Task<bool> UpdateMusicTrackAsync(string trackId, NewMusicTrackModelDTO musicTrackModel)
        {
            var track = await mongoService.GetTrackByIdAsync(trackId);

            if (track != null)
            {
                var styles = await mongoService.GetMusicStylesAsync();
                var style = styles.FirstOrDefault(s => s.Id.ToString() == musicTrackModel.StyleId);

                track.Title = musicTrackModel.Title;
                track.Style = style ?? track.Style;
                track.CreationDate = DateTime.Now;

                if (musicTrackModel.TrackImage != null && musicTrackModel.TrackImage.Length > 0)
                {
                    track.TrackImage = await CompressService.CompressImageFileAsync(musicTrackModel.TrackImage);
                }

                var result = await mongoService.UpdateTrackAsync(track);

                if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
                {
                    // Update in music file storage - google drive
                    await driveAppConnectorService.UpdateFile(musicTrackModel.Mp3File, trackId);
                }

                return result;
            }

            return false;
        }

        public async Task<bool> DeleteMusicTrackAsync(string trackId)
        {
            var result = await mongoService.DeleteTrackByIdAsync(trackId);

            if (result)
            {
                // Delete from music file storage - google drive
                return await driveAppConnectorService.DeleteFile(trackId);
            }

            return false;
        }

        public async Task<bool> DeleteLikedTrackAsync(string userId, string trackId)
        {
            return await mongoService.DeleteTrackFromLikedTracksAsync(userId, trackId);
        }

        /// <summary>
        /// Creates a collection of <see cref="MusicTrackModelDTO"/> objects from a collection of <see cref="MusicTrack"/> objects.
        /// </summary>
        /// <param name="musicTracks">The collection of <see cref="MusicTrack"/> objects to convert.</param>
        /// <param name="userId">The identifier of the user, used to fetch additional author information if provided.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, containing an enumerable of <see cref="MusicTrackModelDTO"/> objects.
        /// </returns>>
        private async Task<IEnumerable<MusicTrackModelDTO>> CreateMusicDTOCollection(IEnumerable<MusicTrack> musicTracks, string? userId)
        {
            var musicTrackDtos = new List<MusicTrackModelDTO>();

            foreach (var musicTrack in musicTracks)
            {
                if (userId != null)
                {
                    var currentAuthor = await mongoService.GetAuthorByIdAsync(userId);
                    if (currentAuthor != null)
                    {
                        musicTrackDtos.Add(new MusicTrackModelDTO(musicTrack, currentAuthor));
                    }
                }
                else
                {
                    musicTrackDtos.Add(new MusicTrackModelDTO(musicTrack));
                }
            }

            return musicTrackDtos;
        }
    }
}
