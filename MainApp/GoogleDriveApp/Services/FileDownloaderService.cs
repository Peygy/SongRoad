using Google.Protobuf;
using GoogleDriveApp.Protos;
using Grpc.Core;

namespace GoogleDriveApp.Services
{
    public class FileDownloaderService : FileDownloader.FileDownloaderBase
    {
        private readonly IGoogleDriveApi driveApi;

        public FileDownloaderService(IGoogleDriveApi driveApi)
        {
            this.driveApi = driveApi;
        }

        public override async Task DownloadFileStream(IAsyncStreamReader<DownloadRequest> requestStream,
            IServerStreamWriter<DownloadResponse> responseStream,
            ServerCallContext context)
        {
            Stream? fileStream = null;

            var readTask = Task.Run(async () =>
            {
                await foreach (DownloadRequest message in requestStream.ReadAllAsync())
                {
                    Console.WriteLine($"Client: {message.FileId}");
                    fileStream = await driveApi.DownloadFile(message.FileId);
                }
            });

            if (!readTask.IsCompleted)
            {
                if (fileStream != null)
                {
                    fileStream.Position = 0;

                    byte[] buffer;
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        buffer = memoryStream.ToArray();
                    }
                    await responseStream.WriteAsync(new DownloadResponse { FileData = ByteString.CopyFrom(buffer) });
                }
                else
                {
                    await responseStream.WriteAsync(new DownloadResponse { FileData = null });

                }
            }

            await readTask;
        }
    }
}
