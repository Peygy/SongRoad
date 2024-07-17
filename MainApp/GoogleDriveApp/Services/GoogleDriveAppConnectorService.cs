using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GoogleDriveApp.Protos;
using Grpc.Core;

namespace GoogleDriveApp.Services
{
    public class GoogleDriveAppConnectorService : GoogleDriveConnector.GoogleDriveConnectorBase
    {
        private readonly IGoogleDriveApi driveApi;

        public GoogleDriveAppConnectorService(IGoogleDriveApi driveApi)
        {
            this.driveApi = driveApi;
        }

        public override Task<Empty> UploadFile(UploadRequest request, ServerCallContext context)
        {
            var byteArray = request.FileStream.ToByteArray();

            var fileStream = new MemoryStream(byteArray);
            if (fileStream == null)
            {
                return Task.FromResult(new Empty());
            }
            fileStream.Position = 0;

            Task.Run(async () =>
            {
                try
                {
                    await driveApi.UploadFile(fileStream, request.FileId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                }
                finally
                {
                    fileStream.Dispose();
                }
            });

            return Task.FromResult(new Empty());
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

        public override Task<Empty> UpdateFile(UpdateRequest request, ServerCallContext context)
        {
            var byteArray = request.FileStream.ToByteArray();

            var fileStream = new MemoryStream(byteArray);
            if (fileStream == null)
            {
                return Task.FromResult(new Empty());
            }
            fileStream.Position = 0;

            Task.Run(async () =>
            {
                try
                {
                    await driveApi.UpdateFile(fileStream, request.FileId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                }
                finally
                {
                    fileStream.Dispose();
                }
            });

            return Task.FromResult(new Empty());
        }

        public override async Task<DeleteResponse> DeleteFile(DeleteRequest request, ServerCallContext context)
        {
            var result = await driveApi.DeleteFile(request.FileId);
            return new DeleteResponse
            {
                State = result
            };
        }
    }
}
