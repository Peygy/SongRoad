using Microsoft.Extensions.Caching.Distributed;

namespace GoogleDriveApp.Services
{
    public interface ITracksCachingService
    {
        Task SetAsync(string fileId, Stream fileStream);
        Task<Stream?> GetAsync(string fileId);
        Task RefreshAsync(string fileId);
        Task DeleteAsync(string fileId);
    }

    public class TracksCachingService : ITracksCachingService
    {
        private readonly IDistributedCache cache;

        public TracksCachingService(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task SetAsync(string fileId, Stream fileStream)
        {
            fileStream.Position = 0;

            var buffer = new byte[fileStream.Length];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);

            await cache.SetAsync(fileId, buffer, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(10)
            });
        }

        public async Task<Stream?> GetAsync(string fileId)
        {
            var cachedData = await cache.GetAsync(fileId);

            if (cachedData != null)
            {
                var fileStream = new MemoryStream(cachedData);
                await SetAsync(fileId, fileStream);
                return fileStream;
            }

            return null;
        }

        public async Task RefreshAsync(string fileId)
        {
            await cache.RefreshAsync(fileId);
        }

        public async Task DeleteAsync(string fileId)
        {
            await cache.RemoveAsync(fileId);
        }
    }
}
