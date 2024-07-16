using Microsoft.Extensions.Caching.Distributed;

namespace MainApp.Services.Music
{
    public interface ITracksCachingService
    {
        Task SetStreamAsync(string fileId, Stream fileStream);
        Task<Stream?> GetStreamAsync(string fileId);
    }

    public class TracksCachingService : ITracksCachingService
    {
        private readonly IDistributedCache cache;

        public TracksCachingService(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task SetStreamAsync(string fileId, Stream fileStream)
        {
            var buffer = new byte[fileStream.Length];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);

            await cache.SetAsync(fileId, buffer, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(10)
            });
        }

        public async Task<Stream?> GetStreamAsync(string fileId)
        {
            var cachedData = await cache.GetAsync(fileId);

            if (cachedData != null)
            {
                return new MemoryStream(cachedData);
            }

            return null;
        }
    }
}
