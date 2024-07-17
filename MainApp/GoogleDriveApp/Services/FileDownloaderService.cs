using Google.Protobuf;
using GoogleDriveApp.Protos;
using Grpc.Core;
using System.IO;

namespace GoogleDriveApp.Services
{
    public class FileDownloaderService : FileDownloader.FileDownloaderBase
    {
        private readonly IGoogleDriveApi driveApi;

        public FileDownloaderService(IGoogleDriveApi driveApi)
        {
            this.driveApi = driveApi;
        }

        public override async Task<DownloadResponse> DownloadFileStream(DownloadRequest request, ServerCallContext context)
        {
            var fileStream = await driveApi.DownloadFile(request.FileId);
            ByteString? bytes;

            if (fileStream != null)
            {
                fileStream.Position = 0;

                var buffer = new byte[fileStream.Length];
                await fileStream.ReadAsync(buffer, 0, buffer.Length);
                bytes = ByteString.CopyFrom(buffer);
            }
            else
            {
                bytes = null;
            }

            return new DownloadResponse
            {
                FileData = bytes
            };
        }
    }
}
