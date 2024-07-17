using Google.Protobuf;
using Grpc.Net.Client;
using MainApp.Protos;
using MainApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers.Api
{
    [Route("api/drive")]
    [ApiController]
    public class ApiDriveController : ControllerBase
    {
        //private readonly IGoogleDriveApi driveApi;

        public ApiDriveController(/*IGoogleDriveApi driveApi*/)
        {
            //this.driveApi = driveApi;
        }

        [HttpGet("download/file")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:7241", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 10 * 1024 * 1024
            });
            var client = new FileDownloader.FileDownloaderClient(channel);

            var downloadRequest = new DownloadRequest { FileId = fileId };
            var downloadResponse = await client.DownloadFileStreamAsync(downloadRequest);
            var byteArray = downloadResponse.FileData.ToByteArray();

            var fileStream = /*await driveApi.DownloadFile(fileId)*/new MemoryStream(byteArray);
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
