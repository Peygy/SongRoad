using MainApp.Data;
using MainApp.Models.Music;
using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MainApp.Services.Music
{
    public interface IMongoService
    {
        Task CheckAuthorExistAsync(UserModel? user);

        Task<string?> AddNewTrackAsync(MusicTrack track, string styleId);
        Task<bool> AddLikedUserTrackAsync(string trackId, string userId);

        Task<MusicAuthor?> GetAuthorByIdAsync(string authorId);
        Task<IEnumerable<MusicTrack>> GetUploadedTracksAsync(string authorId);
        Task<IEnumerable<MusicTrack>> GetLikedTracksAsync(string authorId);
        Task<MusicTrack?> GetTrackByIdAsync(string trackId);
        Task<IEnumerable<Style>> GetMusicStylesAsync();
        Task<IEnumerable<MusicTrack>> GetAllTracksAsync();

        Task<bool> UpdateTrackByIdAsync(MusicTrack updatedTrack);

        Task<bool> DeleteTrackFromLikedTracksAsync(string userId, string trackId);
        Task<bool> DeleteTrackByIdAsync(string trackId);
    }

    /// <summary>
    /// Class of service for actions with MongoDB
    /// </summary>
    public class MongoService : IMongoService
    {
        private readonly MusicContext musicDbContext;

        public MongoService(MusicContext musicDbContext)
        {
            this.musicDbContext = musicDbContext;
        }

        public async Task CheckAuthorExistAsync(UserModel? user)
        {
            if (user != null && !await musicDbContext.MusicAuthors.AnyAsync(s => s.Id == user.Id))
            {
                musicDbContext.MusicAuthors.Add(new MusicAuthor()
                {
                    Id = user.Id,
                    Name = user.UserName
                });
                await musicDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Method for add new music track
        /// </summary>
        /// <param name="track">New music track</param>
        /// <param name="styleId">Chosen music track style</param>
        /// <returns>ID of added music track</returns>
        public async Task<string?> AddNewTrackAsync(MusicTrack track, string styleId)
        {
            if (!await musicDbContext.MusicTracks.AnyAsync(s => s.Title == track.Title))
            {
                track.Style = await musicDbContext.Styles.FindAsync(ObjectId.Parse(styleId));
                musicDbContext.MusicTracks.Add(track);
                await musicDbContext.SaveChangesAsync();

                return track.Id.ToString();
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
            var author = await musicDbContext.MusicAuthors.FindAsync(userId);

            if (author != null)
            {
                var trackObjectId = ObjectId.Parse(trackId);

                if (await musicDbContext.MusicTracks.AnyAsync(m => m.Id == trackObjectId) &&
                    !author.LikedTracks.Any(m => m == trackObjectId))
                {
                    author.LikedTracks.Add(trackObjectId);
                    await musicDbContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<MusicAuthor?> GetAuthorByIdAsync(string authorId)
        {
            return await musicDbContext.MusicAuthors.FindAsync(authorId);
        }

        public async Task<IEnumerable<MusicTrack>> GetUploadedTracksAsync(string authorId)
        {
            var tracks = await musicDbContext.MusicTracks
                .Where(t => t.CreatorId == authorId).ToListAsync();

            foreach (var track in tracks)
            {
                track.Style = await musicDbContext.Styles.FindAsync(track.StyleId);
                track.Creator = await musicDbContext.MusicAuthors.FindAsync(track.CreatorId);
            }

            return tracks;
        }

        public async Task<IEnumerable<MusicTrack>> GetLikedTracksAsync(string authorId)
        {
            var author = await musicDbContext.MusicAuthors.FindAsync(authorId);

            if (author != null)
            {
                var tracks = new List<MusicTrack>();

                foreach (var trackId in author.LikedTracks)
                {
                    var track = await GetTrackByIdAsync(trackId.ToString());
                    if (track != null)
                    {
                        tracks.Add(track);
                    }
                }

                return tracks;
            }

            return new List<MusicTrack>();
        }

        /// <summary>
        /// Method for get music track by id
        /// </summary>
        /// <param name="trackId">music track id</param>
        /// <returns>Music track model</returns>
        public async Task<MusicTrack?> GetTrackByIdAsync(string trackId)
        {
            var track = await musicDbContext.MusicTracks.FindAsync(ObjectId.Parse(trackId));

            if (track != null)
            {
                track.Style = await musicDbContext.Styles.FindAsync(track.StyleId);
                track.Creator = await musicDbContext.MusicAuthors.FindAsync(track.CreatorId);
            }

            return track;
        }

        /// <summary>
        /// Method for get all music tracks styles
        /// </summary>
        /// <returns>List of music tracks styles</returns>
        public async Task<IEnumerable<Style>> GetMusicStylesAsync()
        {
            return await musicDbContext.Styles.ToListAsync();
        }

        public async Task<IEnumerable<MusicTrack>> GetAllTracksAsync()
        {
            var tracks = await musicDbContext.MusicTracks.ToListAsync();

            foreach (var track in tracks)
            {
                track.Style = await musicDbContext.Styles.FindAsync(track.StyleId);
                track.Creator = await musicDbContext.MusicAuthors.FindAsync(track.CreatorId);
            }

            return tracks;
        }

        /// <summary>
        /// Method for update music track data in storage
        /// </summary>
        /// <param name="updatedTrack">Updating model of music track</param>
        /// <returns>Task object</returns>
        /// <exception cref="Exception">Music track not found</exception>
        public async Task<bool> UpdateTrackByIdAsync(MusicTrack updatedTrack)
        {
            musicDbContext.Entry(updatedTrack).State = EntityState.Modified;
            int affectedRows = await musicDbContext.SaveChangesAsync();
            return affectedRows > 0;
        }

        /// <summary>
        /// Method for deleting music track data from storage
        /// </summary>
        /// <param name="trackId">Deleting music track id</param>
        /// <returns>Result of deleting in boolean</returns>
        public async Task<bool> DeleteTrackByIdAsync(string trackId)
        {
            var trackObjectId = ObjectId.Parse(trackId);
            var musicTrack = await musicDbContext.MusicTracks.FindAsync(trackObjectId);

            if (musicTrack != null)
            {
                musicDbContext.MusicTracks.Remove(musicTrack);

                foreach (var author in musicDbContext.MusicAuthors)
                {
                    author.LikedTracks.Remove(trackObjectId);
                }

                await musicDbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteTrackFromLikedTracksAsync(string userId, string trackId)
        {
            var author = await musicDbContext.MusicAuthors.FindAsync(userId);

            if (author != null)
            {
                author.LikedTracks.Remove(ObjectId.Parse(trackId));
                await musicDbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
