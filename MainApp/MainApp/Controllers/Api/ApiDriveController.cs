using MainApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    [Route("api/drive")]
    [ApiController]
    public class ApiDriveController : ControllerBase
    {
        private readonly IGoogleDriveApi driveApi;

        public ApiDriveController(IGoogleDriveApi driveApi)
        {
            this.driveApi = driveApi;
        }

        [HttpGet("download/file")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            var fileStream = await driveApi.DownloadFile(fileId);
            if (fileStream == null)
            {
                return NotFound();
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
    }
}
