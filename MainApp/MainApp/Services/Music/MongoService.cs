﻿using MainApp.Data;
using MainApp.Models.Music;
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

        public MongoService(IOptions<MusicContext> mongoContext, IConfiguration configuration)
        {
            var client = new MongoClient(mongoContext.Value.ConnectionURL);
            var database = client.GetDatabase(mongoContext.Value.DatabaseName);

            tracksCollection = database.GetCollection<MusicTrack>(mongoContext.Value.CollectionNames.First(x => x == "tracks"));
            stylesCollection = database.GetCollection<Style>(mongoContext.Value.CollectionNames.First(x => x == "styles"));
            albumsCollection = database.GetCollection<Album>(mongoContext.Value.CollectionNames.First(x => x == "albums"));
            userTracksCollection = database.GetCollection<UserTracks>(mongoContext.Value.CollectionNames.First(x => x == "user_tracks"));

            var styles = configuration.GetSection("Music:Styles").Get<string[]>();
            _ = InitMusicStylesCollection(styles);
        }

        /// <summary>
        /// Method for add new music track
        /// </summary>
        /// <param name="track">New music track</param>
        /// <param name="style">Chosen music track style</param>
        /// <returns>Task object</returns>
        public async Task AddNewTrackAsync(MusicTrack track, string style)
        {
            if (!tracksCollection.Find(s => s.Title == track.Title).Any())
            {
                track.Style = await stylesCollection.Find(s => s.Id == style).FirstOrDefaultAsync();
                await tracksCollection.InsertOneAsync(track);
            }
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

        public async Task<List<MusicTrack>> GetAllTracksAsync()
        {
            return await tracksCollection.Find(_ => true).ToListAsync();
        }

        public async Task<MusicTrack?> GetTrackByIdAsync(string id)
        {
            return await tracksCollection.Find(track => track.Id == id).FirstOrDefaultAsync();
        }

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
