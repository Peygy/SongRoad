﻿using MainApp.Data;
using MainApp.Models.Music;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MainApp.Services
{
    /// <summary>
    /// Class of service for actions with MongoDB
    /// </summary>
    public class MongoService
    {
        private readonly IMongoCollection<MusicTrack> tracksCollection;
        private readonly IMongoCollection<Style> stylesCollection;
        private readonly IMongoCollection<Album> albumsCollection;
        private readonly IMongoCollection<UserTracks> userTracksCollection;
        private readonly IMongoCollection<TrackImageModel> tracksImagesCollection;

        public MongoService(IOptions<MusicContext> mongoContext, IConfiguration configuration)
        {
            var client = new MongoClient(mongoContext.Value.ConnectionURL);
            var database = client.GetDatabase(mongoContext.Value.DatabaseName);

            tracksCollection = database.GetCollection<MusicTrack>(mongoContext.Value.CollectionNames.First(x => x == "tracks"));
            stylesCollection = database.GetCollection<Style>(mongoContext.Value.CollectionNames.First(x => x == "styles"));
            albumsCollection = database.GetCollection<Album>(mongoContext.Value.CollectionNames.First(x => x == "albums"));
            userTracksCollection = database.GetCollection<UserTracks>(mongoContext.Value.CollectionNames.First(x => x == "user_tracks"));
            tracksImagesCollection = database.GetCollection<TrackImageModel>(mongoContext.Value.CollectionNames.First(x => x == "tracks_images"));

            var styles = configuration.GetSection("Music:Styles").Get<string[]>();
            _ = InitMusicStylesCollectionAsync(styles);
        }

        /// <summary>
        /// Method for add new music track
        /// </summary>
        /// <param name="track">New music track</param>
        /// <param name="style">Chosen music track style</param>
        /// <returns>ID of added music track</returns>
        public async Task<string?> AddNewTrackAsync(MusicTrack track, string style)
        {
            if (!tracksCollection.Find(s => s.Title == track.Title).Any())
            {
                track.Style = await stylesCollection.Find(s => s.Id == style).FirstOrDefaultAsync();
                await tracksCollection.InsertOneAsync(track);
                return track.Id;
            }
            return null;
        }

        /// <summary>
        /// Method for add music track image file to storage
        /// </summary>
        /// <param name="imageFile">Music file</param>
        /// <returns>Model of music track image</returns>
        public async Task<TrackImageModel> AddMusicTrackImageAsync(IFormFile imageFile)
        {
            var compressedImage = await CompressImageFileAsync(imageFile);
            await tracksImagesCollection.InsertOneAsync(compressedImage);

            return compressedImage;
        }

        /// <summary>
        /// Method for add new liked music track to user liked collection
        /// </summary>
        /// <param name="title">Title of liked music track</param>
        /// <param name="userId">Id of current user</param>
        /// <returns>Task object</returns>
        public async Task AddLikedUserTrackAsync(string title, string userId)
        {
            var track = await tracksCollection.Find(s => s.Title == title).FirstOrDefaultAsync();
            var userTracksModel = await userTracksCollection.Find(s => s.UserId == userId).FirstOrDefaultAsync();

            if (userTracksModel != null)
            {
                var update = Builders<UserTracks>.Update.Push(u => u.Tracks, track);
                await userTracksCollection.UpdateOneAsync(s => s.UserId == userId, update);
            }
            else
            {
                var newUserTracksModel = new UserTracks
                {
                    UserId = userId,
                    Tracks = new List<MusicTrack> { track }
                };
                await userTracksCollection.InsertOneAsync(newUserTracksModel);
            }
        }

        /// <summary>
        /// Method for get all tracks
        /// </summary>
        /// <returns>List of music tracks data</returns>
        public async Task<List<MusicTrack>> GetAllTracksAsync()
        {
            return await tracksCollection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Method for get music track by id
        /// </summary>
        /// <param name="trackId">music track id</param>
        /// <returns>Music track model</returns>
        public async Task<MusicTrack?> GetTrackByIdAsync(string trackId)
        {
            return await tracksCollection.Find(track => track.Id == trackId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Method for get all music tracks styles
        /// </summary>
        /// <returns>List of music tracks styles</returns>
        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await stylesCollection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Method for update music track data in storage
        /// </summary>
        /// <param name="updatedTrack">Updating model of music track</param>
        /// <returns>Task object</returns>
        /// <exception cref="Exception">Music track not found</exception>
        public async Task UpdateTrackByIdAsync(MusicTrack updatedTrack)
        {
            // Update model fields
            var update = Builders<MusicTrack>.Update
                .Set(track => track.Title, updatedTrack.Title)
                .Set(track => track.Style, updatedTrack.Style)
                .Set(track => track.CreationDate, updatedTrack.CreationDate)
                .Set(track => track.TrackImage, updatedTrack.TrackImage);

            // Updating
            var updateResult = await tracksCollection.UpdateOneAsync(
                track => track.Id == updatedTrack.Id,
                update
            );

            if (updateResult.MatchedCount == 0)
            {
                throw new Exception("Track not found");
            }
        }

        /// <summary>
        /// Method for update music track image
        /// </summary>
        /// <param name="musicTrack">Music track object</param>
        /// <param name="imageFile">New image file model</param>
        /// <returns>Task object</returns>
        /// <exception cref="Exception">Update of image file wws failed</exception>
        public async Task UpdateMusicTrackImageAsync(MusicTrack musicTrack, IFormFile imageFile)
        {
            var compressedImage = await CompressImageFileAsync(imageFile);

            if (compressedImage.ImageData.Length > 0 && musicTrack.TrackImage.ImageData.Length != compressedImage.ImageData.Length)
            {
                musicTrack.TrackImage.ContentType = compressedImage.ContentType;
                musicTrack.TrackImage.ImageData = compressedImage.ImageData;

                // Update image data fields
                var update = Builders<TrackImageModel>.Update
                    .Set(data => data.ContentType, musicTrack.TrackImage.ContentType)
                    .Set(data => data.ImageData, musicTrack.TrackImage.ImageData);

                // Updating
                var updateResult = await tracksImagesCollection.UpdateOneAsync(
                    image => image.Id == musicTrack.TrackImage.Id,
                    update
                );

                if (updateResult.MatchedCount == 0)
                {
                    throw new Exception("Update image failed");
                }
            }
        }

        /// <summary>
        /// Method for deleting music track data from storage
        /// </summary>
        /// <param name="trackId">Deleting music track id</param>
        /// <returns>Result of deleting in boolean</returns>
        public async Task<bool> DeleteTrackByIdAsync(string trackId)
        {
            var musicTrack = await GetTrackByIdAsync(trackId);
            if (musicTrack != null)
            {
                // Delete music track image
                await DeleteImageTrackByIdAsync(musicTrack.TrackImage.Id);
            }

            var deleteResult = await tracksCollection.DeleteOneAsync(track => track.Id == trackId);

            return deleteResult.DeletedCount > 0;
        }

        /// <summary>
        /// Method for delete music track image
        /// </summary>
        /// <param name="imageId">Music track image id</param>
        /// <returns>Task object</returns>
        private async Task DeleteImageTrackByIdAsync(string imageId)
        {
            await tracksImagesCollection.DeleteOneAsync(image => image.Id == imageId);
        }

        /// <summary>
        /// Method for initialize collection of music styles
        /// </summary>
        /// <param name="styles">Collection of music styles</param>
        /// <returns>Task object</returns>
        private async Task InitMusicStylesCollectionAsync(string[] styles)
        {
            if ((await stylesCollection.Find(_ => true).ToListAsync()).Count != 0)
            {
                return;
            }

            var styleList = new List<Style>();

            foreach (var styleName in styles)
            {
                var style = new Style
                {
                    Name = styleName,
                };

                styleList.Add(style);
            }

            await stylesCollection.InsertManyAsync(styleList, new InsertManyOptions { IsOrdered = false });
        }

        /// <summary>
        /// Method for compress image file
        /// </summary>
        /// <param name="imageFile">Image file</param>
        /// <returns>Compressed music track image object</returns>
        private async Task<TrackImageModel> CompressImageFileAsync(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                using var inputStream = imageFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                var encoder = new JpegEncoder
                {
                    Quality = 75
                };

                using var outputStream = new MemoryStream();
                await image.SaveAsJpegAsync(outputStream, encoder);

                return new TrackImageModel
                {
                    ContentType = "image/jpeg",
                    ImageData = outputStream.ToArray()
                };
            }

            return new TrackImageModel();
        }
    }
}
