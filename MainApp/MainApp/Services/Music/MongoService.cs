using MainApp.Data;
using MainApp.Models.Music;
using MainApp.Models.User;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MainApp.Services.Music
{
    /// <summary>
    /// Defines the contract for a service that interacts with a MongoDB database.
    /// </summary>
    public interface IMongoService
    {
        /// <summary>
        /// Checks if the <paramref name="user"/> exists as an author.
        /// </summary>
        /// <param name="user">User who mades request.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task CheckAuthorExistAsync(UserModel? user);

        /// <summary>
        /// Adds a new music track <paramref name="track"/> to the database, 
        /// with choisen style by <paramref name="styleId"/>.
        /// </summary>
        /// <param name="track">The music track to add.</param>
        /// <param name="styleId">The specific identification of the choisen style</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing the identification of the added music track.
        /// </returns>
        Task<string?> AddNewTrackAsync(MusicTrack track, string styleId);
        /// <summary>
        /// Adds a track, which has specific identifier <paramref name="trackId"/>, 
        /// to the liked tracks of a user, who has specific identifier <paramref name="userId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to be liked.</param>
        /// <param name="userId">The identifier of the user who liked the track.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the operation was successful.
        /// </returns>
        Task<bool> AddLikedUserTrackAsync(string trackId, string userId);

        /// <summary>
        /// Gets an author by their identifier <paramref name="authorId"/>.
        /// </summary>
        /// <param name="authorId">The identifier of the author.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing the <see cref="MusicAuthor"/> if found, or null if not.
        /// </returns>
        Task<MusicAuthor?> GetAuthorByIdAsync(string authorId);
        /// <summary>
        /// Gets the uploaded tracks of a specific author, 
        /// who has specific identifier <paramref name="authorId"/>.
        /// </summary>
        /// <param name="authorId">The identifier of the author.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="MusicTrack"/> objects.
        /// </returns>
        Task<IEnumerable<MusicTrack>> GetUploadedTracksByAuthorIdAsync(string authorId);
        /// <summary>
        /// Gets the liked tracks of a specific author,
        /// who has specific identifier <paramref name="authorId"/>.
        /// </summary>
        /// <param name="authorId">The identifier of the author.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="MusicTrack"/> objects.
        /// </returns>
        Task<IEnumerable<MusicTrack>> GetLikedTracksByAuthorIdAsync(string authorId);
        /// <summary>
        /// Gets a music track by its identifier <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing the <see cref="MusicTrack"/> if found, or null if not.
        /// </returns>
        Task<MusicTrack?> GetTrackByIdAsync(string trackId);
        /// <summary>
        /// Gets all available music styles.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="Style"/> objects.
        /// </returns>
        Task<IEnumerable<Style>> GetMusicStylesAsync();
        /// <summary>
        /// Gets all music tracks.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing an enumerable of <see cref="MusicTrack"/> objects.
        /// </returns>
        Task<IEnumerable<MusicTrack>> GetAllTracksAsync();

        /// <summary>
        /// Updates the specified music track <paramref name="updatedTrack"/>.
        /// </summary>
        /// <param name="updatedTrack">The updated music track object.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the update was successful.
        /// </returns>
        Task<bool> UpdateTrackAsync(MusicTrack updatedTrack);

        /// <summary>
        /// Deletes a music track by its identifier <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to be deleted.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a boolean value indicating whether the deletion was successful.
        /// </returns>
        Task<bool> DeleteTrackByIdAsync(string trackId);
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
        Task<bool> DeleteTrackFromLikedTracksAsync(string userId, string trackId);
    }

    /// <inheritdoc cref="IMongoService">
    public class MongoService : IMongoService
    {
        /// <summary>
        /// The database context used for accessing the music data.
        /// </summary>
        private readonly MusicContext musicDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoService"/> class.
        /// </summary>
        /// <param name="musicDbContext">The database context used for accessing the music data.</param>
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

        public async Task<bool> AddLikedUserTrackAsync(string trackId, string userId)
        {
            var author = await musicDbContext.MusicAuthors.FindAsync(userId);

            if (author != null)
            {
                // Parse string value into ObjcetId
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

        public async Task<IEnumerable<MusicTrack>> GetUploadedTracksByAuthorIdAsync(string authorId)
        {
            // List of tracks, where CreatorId is authorId
            var tracks = await musicDbContext.MusicTracks
                .Where(t => t.CreatorId == authorId).ToListAsync();

            foreach (var track in tracks)
            {
                track.Style = await musicDbContext.Styles.FindAsync(track.StyleId);
                track.Creator = await musicDbContext.MusicAuthors.FindAsync(track.CreatorId);
            }

            return tracks;
        }

        public async Task<IEnumerable<MusicTrack>> GetLikedTracksByAuthorIdAsync(string authorId)
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

        public async Task<bool> UpdateTrackAsync(MusicTrack updatedTrack)
        {
            musicDbContext.Entry(updatedTrack).State = EntityState.Modified;
            int affectedRows = await musicDbContext.SaveChangesAsync();
            return affectedRows > 0;
        }

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
