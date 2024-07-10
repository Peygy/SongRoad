using MainApp.Data;
using MainApp.Models.Music;
using MainApp.Models.User;
using MainApp.Services.Music;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MainApp.Services
{
    public interface IMongoService
    {
        Task CheckAuthorExistAsync(UserModel user);

        Task<TrackImageModel> AddMusicTrackImageAsync(IFormFile imageFile);
        Task<string?> AddNewTrackAsync(MusicTrack track, string style);
        Task<bool> AddLikedUserTrackAsync(string trackId, string userId);

        Task<MusicAuthor?> GetAuthorByIdAsync(string authorId);
        Task<MusicTrack?> GetTrackByIdAsync(string trackId);
        Task<List<Style>> GetMusicStylesAsync();
        Task<List<MusicTrack>> GetAllTracksAsync();

        Task UpdateTrackByIdAsync(MusicTrack updatedTrack);
        Task UpdateMusicTrackImageAsync(MusicTrack musicTrack, IFormFile imageFile);

        Task<bool> DeleteTrackFromLikedTracksAsync(string userId, string trackId);
        Task<bool> DeleteTrackByIdAsync(string trackId);
    }

    /// <summary>
    /// Class of service for actions with MongoDB
    /// </summary>
    public class MongoService : IMongoService
    {
        private readonly IMongoCollection<MusicTrack> tracksCollection;
        private readonly IMongoCollection<Style> stylesCollection;
        private readonly IMongoCollection<TrackImageModel> tracksImagesCollection;
        private readonly IMongoCollection<MusicAuthor> musicAuthorsCollection;

        public MongoService(IOptions<MusicContext> mongoContext, IConfiguration configuration)
        {
            var client = new MongoClient(mongoContext.Value.ConnectionURL);
            var database = client.GetDatabase(mongoContext.Value.DatabaseName);

            tracksCollection = database.GetCollection<MusicTrack>(mongoContext.Value.CollectionNames.First(x => x == "tracks"));
            stylesCollection = database.GetCollection<Style>(mongoContext.Value.CollectionNames.First(x => x == "styles"));
            tracksImagesCollection = database.GetCollection<TrackImageModel>(mongoContext.Value.CollectionNames.First(x => x == "tracks_images"));
            musicAuthorsCollection = database.GetCollection<MusicAuthor>(mongoContext.Value.CollectionNames.First(x => x == "music_authors"));

            var styles = configuration.GetSection("Music:Styles").Get<string[]>();
            InitMusicStylesCollectionAsync(styles).GetAwaiter().GetResult();
        }

        public async Task CheckAuthorExistAsync(UserModel user)
        {
            if (!musicAuthorsCollection.Find(s => s.Id == user.Id).Any())
            {
                await musicAuthorsCollection.InsertOneAsync(new MusicAuthor()
                {
                    Id = user.Id,
                    Name = user.UserName
                });
            }
        }

        /// <summary>
        /// Method for add music track image file to storage
        /// </summary>
        /// <param name="imageFile">Music file</param>
        /// <returns>Model of music track image</returns>
        public async Task<TrackImageModel> AddMusicTrackImageAsync(IFormFile imageFile)
        {
            var compressedImage = await CompressService.CompressImageFileAsync(imageFile);
            await tracksImagesCollection.InsertOneAsync(compressedImage);

            return compressedImage;
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
                var update = Builders<MusicAuthor>.Update.Push(a => a.UploadedTracksId, track.Id);

                var updateResult = await musicAuthorsCollection.UpdateOneAsync(
                    a => a.Id == track.CreatorId,
                    update
                );

                return track.Id;
            }

            return null;
        }

        /// <summary>
        /// Method for add new liked music track to user liked collection
        /// </summary>
        /// <param name="trackId">Id of liked music track</param>
        /// <param name="userId">Id of current user</param>
        /// <returns>Task object</returns>
        public async Task<bool> AddLikedUserTrackAsync(string trackId, string userId)
        {
            var update = Builders<MusicAuthor>.Update.Push(a => a.LikedTracksId, trackId);
            var updateResult = await musicAuthorsCollection.UpdateOneAsync(a => a.Id == userId, update);

            return updateResult.MatchedCount > 0;
        }

        /// <summary>
        /// Method for get all tracks
        /// </summary>
        /// <returns>List of music tracks data</returns>
        public async Task<MusicAuthor?> GetAuthorByIdAsync(string authorId)
        {
            return await musicAuthorsCollection.Find(a => a.Id == authorId).FirstOrDefaultAsync();
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

        public async Task<List<MusicTrack>> GetAllTracksAsync()
        {
            return await tracksCollection.Find(_ => true).ToListAsync();
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
        /// <exception cref="Exception">Update of image file was failed</exception>
        public async Task UpdateMusicTrackImageAsync(MusicTrack musicTrack, IFormFile imageFile)
        {
            var compressedImage = await CompressService.CompressImageFileAsync(imageFile);

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
                await DeleteTrackFromAuthorsAsync(musicTrack.CreatorId, trackId);
            }

            var deleteResult = await tracksCollection.DeleteOneAsync(track => track.Id == trackId);

            return deleteResult.DeletedCount > 0;
        }

        public async Task<bool> DeleteTrackFromLikedTracksAsync(string userId, string trackId)
        {
            var update = Builders<MusicAuthor>.Update.Pull(a => a.LikedTracksId, trackId);
            var updateResult = await musicAuthorsCollection.UpdateManyAsync(
                a => a.Id == userId,
                update
            );

            return updateResult.MatchedCount > 0;
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

        private async Task DeleteTrackFromAuthorsAsync(string authorId, string trackId)
        {
            var update = Builders<MusicAuthor>.Update.Pull(a => a.UploadedTracksId, trackId);

            await musicAuthorsCollection.UpdateOneAsync(
                a => a.Id == authorId,
                update
            );

            update = Builders<MusicAuthor>.Update.Pull(a => a.LikedTracksId, trackId);
            await musicAuthorsCollection.UpdateManyAsync(
                Builders<MusicAuthor>.Filter.Empty,
                update
            );
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
    }
}
