using MainApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace MainApp.Controllers.Api
{
    [Route("api/drive")]
    [ApiController]
    public class ApiDriveController : ControllerBase
    {
        private readonly IGoogleDriveApi driveApi;
        private readonly IDistributedCache cache;

        public ApiDriveController(IGoogleDriveApi driveApi, IDistributedCache cache)
        {
            this.driveApi = driveApi;
            this.cache = cache;
        }

        [HttpGet("download/file")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            var fileStream = await GetFileStreamFromCacheAsync(fileId);

            if (fileStream == null)
            {
                fileStream = await driveApi.DownloadFile(fileId);
                if (fileStream == null)
                {
                    return NotFound();
                }

                await CacheFileStreamAsync(fileId, fileStream);
            }

            // Get file length
            long fileLength = fileStream.Length;
            string contentRange = $"bytes 0-{fileLength - 1}/{fileLength}";

            var response = File(fileStream, "audio/mpeg", fileId);

            // Add headers for correct file work
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Length", fileLength.ToString());
            Response.Headers.Add("Content-Range", contentRange);

            return response;
        }

        private async Task<Stream?> GetFileStreamFromCacheAsync(string fileId)
        {
            var cachedData = await cache.GetAsync(fileId);

            if (cachedData != null)
            {
                return new MemoryStream(cachedData);
            }

            return null;
        }

        private async Task CacheFileStreamAsync(string fileId, Stream fileStream)
        {
            var buffer = new byte[fileStream.Length];
            await fileStream.ReadAsync(buffer, 0, buffer.Length);

            await cache.SetAsync(fileId, buffer, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(10)
            });
        }
    }
}
