﻿using MainApp.Interfaces.Music;
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
        private readonly IMongoService mongoService;
        private readonly IGoogleDriveApi driveApi;

        public MusicService(IMongoService mongoService, IGoogleDriveApi driveApi)
        {
            this.mongoService = mongoService;
            this.driveApi = driveApi;
        }

        public async Task CheckAuthorExistAsync(UserModel user)
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
                await driveApi.UploadFile(musicTrackModel.Mp3File, trackId);
                return true;
            }

            return false;
        }

        public async Task AddLikedTrackAsync(string trackId, string userId)
        {
            await mongoService.AddLikedUserTrackAsync(trackId, userId);
        }

        /// <summary>
        /// Method for get list of user's personal (where he's creator) files on google drive - cloud storage
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of DTO music tracks models</returns>
        public async Task<List<MusicTrackModelDTO>> GetUserUploadedTrackListAsync(string userId)
        {
            var authorModel = await mongoService.GetAuthorByIdAsync(userId);
            var musicTracks = new List<MusicTrackModelDTO>();

            if (authorModel != null)
            {
                foreach (var musicTrackId in authorModel.UploadedTracksId)
                {
                    var musicTrack = await mongoService.GetTrackByIdAsync(musicTrackId);
                    if (musicTrack != null)
                    {
                        musicTracks.Add(new MusicTrackModelDTO(musicTrack, authorModel));
                    }
                }
            }

            return musicTracks;
        }

        /// <summary>
        /// Method for get music track model by id
        /// </summary>
        /// <param name="trackId">Music track id</param>
        /// <returns>DTO model of music track</returns>
        public async Task<T?> GetMusicTrackByIdAsync<T>(string trackId) where T : class
        {
            var musicTrackModel = await mongoService.GetTrackByIdAsync(trackId);
            if (musicTrackModel != null)
            {
                if (typeof(T) == typeof(MusicTrackModelDTO))
                {
                    return new MusicTrackModelDTO(musicTrackModel) as T;
                }
                else
                {
                    return musicTrackModel as T;
                }
            }

            return null;
        }

        public async Task<List<MusicTrackModelDTO>> GetAllLikedMusicTracksAsync(string userId)
        {
            var authorModel = await mongoService.GetAuthorByIdAsync(userId);
            var musicTracks = new List<MusicTrackModelDTO>();

            if (authorModel != null)
            {
                foreach (var musicTrackId in authorModel.LikedTracksId)
                {
                    var musicTrack = await mongoService.GetTrackByIdAsync(musicTrackId);
                    if (musicTrack != null)
                    {
                        var likeedMusicTrack = new MusicTrackModelDTO(musicTrack);
                        // For improve perfomance
                        likeedMusicTrack.isLiked = true;
                        likeedMusicTrack.CreatorName = authorModel.Name;

                        musicTracks.Add(likeedMusicTrack);
                    }
                }
            }

            return musicTracks;
        }

        public async Task<IEnumerable<MusicTrack>> GetAllMusicTracksAsync()
        {
            return await mongoService.GetAllTracksAsync();
        }

        /// <summary>
        /// Method for get list of music track's styles
        /// </summary>
        /// <returns>List of music styles</returns>
        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await mongoService.GetMusicStylesAsync();
        }

        public async Task<List<MusicTrackModelDTO>> GetMusicTracksForViewAsync(string? userId)
        {
            var tracks = await mongoService.GetAllTracksAsync();
            MusicAuthor? authorModel = null;

            if (userId != null)
            {
                authorModel = await mongoService.GetAuthorByIdAsync(userId);
            }

            return tracks.Select(track => authorModel != null
                ? new MusicTrackModelDTO(track, authorModel)
                : new MusicTrackModelDTO(track)).ToList();
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
                var updateImageTask = mongoService.UpdateMusicTrackImageAsync(track, musicTrackModel.TrackImage);
                var stylesTask = mongoService.GetMusicStylesAsync();
                await Task.WhenAll(updateImageTask, stylesTask);

                var style = stylesTask.Result.FirstOrDefault(s => s.Id == musicTrackModel.Style);

                track.Title = musicTrackModel.Title;
                track.Style = style ?? track.Style;
                track.CreationDate = DateTime.Now;

                // Update music data to mongo storage
                await mongoService.UpdateTrackByIdAsync(track);

                if (musicTrackModel.Mp3File != null && musicTrackModel.Mp3File.Length > 0)
                {
                    await driveApi.UpdateFile(musicTrackModel.Mp3File, trackId);
                }
            }
            else
            {
                throw new Exception("Track not found");
            }
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
    }
}
