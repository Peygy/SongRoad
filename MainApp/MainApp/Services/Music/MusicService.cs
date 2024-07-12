using MainApp.Interfaces.Music;
using MainApp.Models.User;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Authorization;
using MainApp.DTO.Music;
using MainApp.Services.Music;

namespace MainApp.Services
{
    /// <summary>
    /// Class of service for actions with music tracks
    /// </summary>
    [Authorize(Roles = UserRoles.User)]
    public class MusicService : IMusicService
    {
        private readonly IMongoService mongoService;
        private readonly IGoogleDriveApi driveApi;

        public MusicService(IMongoService mongoService, IGoogleDriveApi driveApi)
        {
            this.mongoService = mongoService;
            this.driveApi = driveApi;
            // userId здесь обозначать и CheckAuthorExistAsync
        }

        public async Task CheckAuthorExistAsync(UserModel? user)
        {
            await mongoService.CheckAuthorExistAsync(user);
        }

        /// <summary>
        /// Method for add new music track
        /// </summary>
        /// <param name="musicTrackModel">New music track DTO model</param>
        /// <param name="userId">Current user id</param>
        /// <returns>Task object</returns>
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
                // Add to file storage - drive
                await driveApi.UploadFile(musicTrackModel.Mp3File, trackId);
                return true;
            }

            return false;
        }

        public async Task<bool> AddLikedTrackAsync(string trackId, string userId)
        {
            return await mongoService.AddLikedUserTrackAsync(trackId, userId);
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
                return new MusicTrackModelDTO(musicTrackModel);
            }

            return null;
        }

        public async Task<IEnumerable<MusicTrackModelDTO>> GetAllMusicTracksAsync(string? userId)
        {
            var musicTracks = await mongoService.GetAllTracksAsync();

            return await CreateMusicDTOCollection(musicTracks, userId);
        }

        /// <summary>
        /// Method for get list of user's personal (where he's creator) files on google drive - cloud storage
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of DTO music tracks models</returns>
        public async Task<IEnumerable<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId)
        {
            var musicTracks = await mongoService.GetUploadedTracksAsync(userId);

            return await CreateMusicDTOCollection(musicTracks, userId);
        }

        public async Task<IEnumerable<MusicTrackModelDTO>> GetAllLikedMusicTracksAsync(string userId)
        {
            var musicTracks = await mongoService.GetLikedTracksAsync(userId);

            return await CreateMusicDTOCollection(musicTracks, userId);
        }

        /// <summary>
        /// Method for get list of music track's styles
        /// </summary>
        /// <returns>List of music styles</returns>
        public async Task<IEnumerable<Style>> GetMusicStylesAsync()
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
        public async Task<bool> UpdateMusicTrackAsync(string trackId, NewMusicTrackModelDTO musicTrackModel)
        {
            var track = await mongoService.GetTrackByIdAsync(trackId);

            if (track != null)
            {
                var styles = await mongoService.GetMusicStylesAsync();
                var style = styles.FirstOrDefault(s => s.Id == musicTrackModel.StyleId);

                track.Title = musicTrackModel.Title;
                track.Style = style ?? track.Style;
                track.CreationDate = DateTime.Now;

                if (musicTrackModel.TrackImage != null && musicTrackModel.TrackImage.Length > 0)
                {
                    track.TrackImage = await CompressService.CompressImageFileAsync(musicTrackModel.TrackImage);
                }

                var result = await mongoService.UpdateTrackByIdAsync(track);

                if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
                {
                    await driveApi.UpdateFile(musicTrackModel.Mp3File, trackId);
                }

                return result;
            }

            return false;
        }

        /// <summary>
        /// Method for delete music track from storages
        /// </summary>
        /// <param name="trackId">Music track id</param>
        /// <returns>Result of deleting in boolean</returns>
        public async Task<bool> DeleteMusicTrackAsync(string trackId)
        {
            var result = await mongoService.DeleteTrackByIdAsync(trackId);

            if (result)
            {
                return await driveApi.DeleteFile(trackId);
            }

            return false;
        }

        public async Task<bool> DeleteLikedTrackAsync(string userId, string trackId)
        {
            return await mongoService.DeleteTrackFromLikedTracksAsync(userId, trackId);
        }


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
