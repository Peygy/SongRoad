using Google.Protobuf;
using Grpc.Net.Client;
using MainApp.Protos;

namespace MainApp.Services.Music
{
    public interface IGoogleDriveAppConnectorService
    {
        Task UploadFile(IFormFile mp3File, string trackId);
        Task<Stream?> DownloadFile(string trackId);
        Task UpdateFile(IFormFile mp3File, string trackId);
        Task<bool> DeleteFile(string trackId);
    }

    public class GoogleDriveAppConnectorService : IGoogleDriveAppConnectorService
    {
        private readonly GoogleDriveConnector.GoogleDriveConnectorClient client;

        public GoogleDriveAppConnectorService(IConfiguration configuration)
        {
            var serviceAddress = configuration.GetSection("GoogleDriveAppAddress").Value ?? string.Empty;
            var channel = GrpcChannel.ForAddress(serviceAddress, new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 16 * 1024 * 1024,
                MaxSendMessageSize = 16 * 1024 * 1024
            });
            client = new GoogleDriveConnector.GoogleDriveConnectorClient(channel);
        }

        public async Task UploadFile(IFormFile mp3File, string trackId)
        {
            var request = new UploadRequest { FileStream = ConvertToByteString(mp3File), FileId = trackId };
            await client.UploadFileAsync(request);
        }

        public async Task<Stream?> DownloadFile(string trackId)
        {
            var request = new DownloadRequest { FileId = trackId };
            var response = await client.DownloadFileStreamAsync(request);
            var byteArray = response.FileData.ToByteArray();

            return new MemoryStream(byteArray);
        }

        public async Task UpdateFile(IFormFile mp3File, string trackId)
        {
            var request = new UpdateRequest { FileStream = ConvertToByteString(mp3File), FileId = trackId };
            await client.UpdateFileAsync(request);
        }

        public async Task<bool> DeleteFile(string trackId)
        {
            var request = new DeleteRequest { FileId = trackId };
            return (await client.DeleteFileAsync(request)).State;
        }

        private ByteString ConvertToByteString(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                formFile.CopyTo(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                return ByteString.CopyFrom(fileBytes);
            }
        }
    }
}