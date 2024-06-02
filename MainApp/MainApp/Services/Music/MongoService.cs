using MainApp.Data;
using MainApp.Models.Music;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MainApp.Services
{
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

        public async Task AddNewTrackAsync(MusicTrack track, string style)
        {
            if (!tracksCollection.Find(s => s.Title == track.Title).Any())
            {
                track.Style = await stylesCollection.Find(s => s.Name == style).FirstOrDefaultAsync();
                await tracksCollection.InsertOneAsync(track);
            }
        }
        public async Task AddLikedUserTrackAsync(string title, string userId)
        {
            var track = await tracksCollection.Find(s => s.Title == title).FirstOrDefaultAsync();
            var userTracksModel = await userTracksCollection.Find(s => s.UserId == userId).FirstOrDefaultAsync();

            if (userTracksModel != null)
            {
                await userTracksCollection.UpdateOneAsync(
                    s => s.UserId == userId,
                    new BsonDocument("$push", new BsonDocument { { "Tracks", track.ToBson() } })
                );
            }
            else
            {

            }

            //await userTracksCollection.InsertOneAsync();
        }

        public async Task GetAllTracksAsync()
        {

        }

        public async Task GetTrackByIdAsync()
        {

        }

        public async Task UpdateTrackByIdAsync()
        {

        }

        public async Task DeleteTrackByIdAsync()
        {

        }

        private async Task InitMusicStylesCollection(string[] styles)
        {
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
