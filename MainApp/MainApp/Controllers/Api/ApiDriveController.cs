using MainApp.Services.Music;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    [Route("api/drive")]
    [ApiController]
    public class ApiDriveController : ControllerBase
    {
        private readonly IGoogleDriveAppConnectorService driveAppConnectorService;

        public ApiDriveController(IGoogleDriveAppConnectorService driveAppConnectorService)
        {
            this.driveAppConnectorService = driveAppConnectorService;
        }

        [HttpGet("download/file")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            var fileStream = await driveAppConnectorService.DownloadFile(fileId);
            if (fileStream == null)
            {
                return NotFound();
            }
            fileStream.Position = 0;

            // Get file length
            long fileLength = fileStream.Length;
            var response = File(fileStream, "audio/mpeg", fileId);

            // Add headers for correct file work
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Length", fileLength.ToString());
            Response.Headers.Add("Content-Range", $"bytes 0-{fileLength - 1}/{fileLength}");

            return response;
        }
    }
}
