using MainApp.Data;
using MainApp.Models.Music;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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
            _ = InitMusicStylesCollection(styles);
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
        public async Task<TrackImageModel> AddMusicTrackImage(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);

                var imageModel = new TrackImageModel
                {
                    ContentType = imageFile.ContentType,
                    ImageData = memoryStream.ToArray()
                };

                await tracksImagesCollection.InsertOneAsync(imageModel);
                return imageModel;
            }

            return new TrackImageModel() { ImageData = Array.Empty<byte>() };
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
        /// <param name="id">music track id</param>
        /// <returns>Music track model</returns>
        public async Task<MusicTrack?> GetTrackByIdAsync(string id)
        {
            return await tracksCollection.Find(track => track.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Method for get all music tracks styles
        /// </summary>
        /// <returns>List of music tracks styles</returns>
        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await stylesCollection.Find(_ => true).ToListAsync();
        }

        public async Task UpdateTrackByIdAsync(string id, MusicTrack updatedTrack)
        {
            var updateResult = await tracksCollection.ReplaceOneAsync(
                track => track.Id == id,
                updatedTrack
            );

            if (updateResult.MatchedCount == 0)
            {
                throw new Exception("Track not found");
            }
        }

        public async Task DeleteTrackByIdAsync(string id)
        {
            var deleteResult = await tracksCollection.DeleteOneAsync(track => track.Id == id);

            if (deleteResult.DeletedCount == 0)
            {
                throw new Exception("Track not found");
            }
        }

        /// <summary>
        /// Method for initialize collection of music styles
        /// </summary>
        /// <param name="styles">Collection of music styles</param>
        /// <returns>Task object</returns>
        private async Task InitMusicStylesCollection(string[] styles)
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
