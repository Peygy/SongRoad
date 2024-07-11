using MainApp.Data;
using MainApp.Models.Music;
using MainApp.Models.User;
using MainApp.Services.Music;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MainApp.Services
{
    public interface IMongoService
    {
        Task CheckAuthorExistAsync(UserModel user);

        Task<TrackImageModel> AddMusicTrackImageAsync(IFormFile imageFile);
        Task<ObjectId?> AddNewTrackAsync(MusicTrack track, ObjectId styleId);
        Task<bool> AddLikedUserTrackAsync(ObjectId trackId, string userId);

        Task<MusicAuthor?> GetAuthorByIdAsync(string authorId);
        Task<MusicTrack?> GetTrackByIdAsync(ObjectId trackId);
        Task<List<Style>> GetMusicStylesAsync();
        Task<List<MusicTrack>> GetAllTracksAsync();

        Task UpdateTrackByIdAsync(MusicTrack updatedTrack);
        Task UpdateMusicTrackImageAsync(MusicTrack musicTrack, IFormFile imageFile);

        Task<bool> DeleteTrackFromLikedTracksAsync(string userId, ObjectId trackId);
        Task<bool> DeleteTrackByIdAsync(ObjectId trackId);
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

        public async Task CheckAuthorExistAsync(UserModel user)
        {
            if (!await musicDbContext.MusicAuthors.AnyAsync(s => s.Id == user.Id))
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
        /// Method for add music track image file to storage
        /// </summary>
        /// <param name="imageFile">Music file</param>
        /// <returns>Model of music track image</returns>
        public async Task<TrackImageModel> AddMusicTrackImageAsync(IFormFile imageFile)
        {
            var compressedImage = await CompressService.CompressImageFileAsync(imageFile);
            musicDbContext.TrackImages.Add(compressedImage);
            await musicDbContext.SaveChangesAsync();

            return compressedImage;
        }

        /// <summary>
        /// Method for add new music track
        /// </summary>
        /// <param name="track">New music track</param>
        /// <param name="style">Chosen music track style</param>
        /// <returns>ID of added music track</returns>
        public async Task<ObjectId?> AddNewTrackAsync(MusicTrack track, ObjectId styleId)
        {
            if (!await musicDbContext.MusicTracks.AnyAsync(s => s.Title == track.Title))
            {
                track.Style = await musicDbContext.Styles.FindAsync(styleId);
                musicDbContext.MusicTracks.Add(track);
                await musicDbContext.SaveChangesAsync();

                var author = await musicDbContext.MusicAuthors.FindAsync(track.CreatorId);
                author.UploadedTracks.Add(track);
                await musicDbContext.SaveChangesAsync();

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
        public async Task<bool> AddLikedUserTrackAsync(ObjectId trackId, string userId)
        {
            var author = await musicDbContext.MusicAuthors.FindAsync(userId);

            if (author != null)
            {
                var likedTrack = await musicDbContext.MusicTracks.FindAsync(trackId);

                if (likedTrack != null)
                {
                    author.LikedTracks.Add(likedTrack);
                    await musicDbContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Method for get all tracks
        /// </summary>
        /// <returns>List of music tracks data</returns>
        public async Task<MusicAuthor?> GetAuthorByIdAsync(string authorId)
        {
            return await musicDbContext.MusicAuthors
                .Include(a => a.UploadedTracks)
                .Include(a => a.LikedTracks)
                .FirstOrDefaultAsync(a => a.Id == authorId);
        }

        /// <summary>
        /// Method for get music track by id
        /// </summary>
        /// <param name="trackId">music track id</param>
        /// <returns>Music track model</returns>
        public async Task<MusicTrack?> GetTrackByIdAsync(ObjectId trackId)
        {
            return await musicDbContext.MusicTracks
                .Include(t => t.Style)
                .Include(t => t.TrackImage)
                .Include(t => t.Creator)
                //.Include(t => t.LikedBy)
                .FirstOrDefaultAsync(t => t.Id == trackId);
        }

        /// <summary>
        /// Method for get all music tracks styles
        /// </summary>
        /// <returns>List of music tracks styles</returns>
        public async Task<List<Style>> GetMusicStylesAsync()
        {
            return await musicDbContext.Styles.ToListAsync();
        }

        public async Task<List<MusicTrack>> GetAllTracksAsync()
        {
            return await musicDbContext.MusicTracks
                .Include(t => t.Style)
                .Include(t => t.TrackImage)
                .Include(t => t.Creator)
                .Include(t => t.LikedBy)
                .ToListAsync();
        }

        /// <summary>
        /// Method for update music track data in storage
        /// </summary>
        /// <param name="updatedTrack">Updating model of music track</param>
        /// <returns>Task object</returns>
        /// <exception cref="Exception">Music track not found</exception>
        public async Task UpdateTrackByIdAsync(MusicTrack updatedTrack)
        {
            musicDbContext.Entry(updatedTrack).State = EntityState.Modified;
            await musicDbContext.SaveChangesAsync();
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

            if (musicTrack.TrackImage.ImageData.Length != compressedImage.ImageData.Length)
            {
                musicTrack.TrackImage = compressedImage;

                musicDbContext.Entry(musicTrack).State = EntityState.Modified;
                await musicDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Method for deleting music track data from storage
        /// </summary>
        /// <param name="trackId">Deleting music track id</param>
        /// <returns>Result of deleting in boolean</returns>
        public async Task<bool> DeleteTrackByIdAsync(ObjectId trackId)
        {
            var musicTrack = await musicDbContext.MusicTracks.FindAsync(trackId);

            if (musicTrack != null)
            {
                musicDbContext.MusicTracks.Remove(musicTrack);
                await musicDbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteTrackFromLikedTracksAsync(string userId, ObjectId trackId)
        {
            var author = await musicDbContext.MusicAuthors.FindAsync(userId);

            if (author != null)
            {
                var likedTrack = await musicDbContext.MusicTracks.FindAsync(trackId);

                if (likedTrack != null)
                {
                    author.LikedTracks.Remove(likedTrack);
                    await musicDbContext.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        /*/// <summary>
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
        }*/
    }
}
