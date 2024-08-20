using Google.Protobuf;
using MainApp.Protos;

namespace MainApp.Services.Music
{
    /// <summary>
    /// Defines the contract for a service that interacts with a Grpc server-microservice GoogleDriveApp.
    /// </summary>
    public interface IGoogleDriveAppConnectorService
    {
        /// <summary>
        /// Uploads new file <paramref name="mp3File"/>, 
        /// which has specific identifier <paramref name="trackId"/>.
        /// </summary>
        /// <param name="mp3File">The mp3 music file to upload.</param>
        /// <param name="trackId">The identifier of the track.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task UploadFile(IFormFile mp3File, string trackId);

        /// <summary>
        /// Downloads the file associated with the specific <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to download.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// with a <see cref="Stream"/> containing the file data, or <c>null</c> if the file is not found.
        /// </returns>
        Task<Stream?> DownloadFile(string trackId);

        /// <summary>
        /// Updates an existing file associated with the specific <paramref name="trackId"/> 
        /// by replacing it with a new <paramref name="mp3File"/>.
        /// </summary>
        /// <param name="mp3File">The new mp3 file to replace the existing one.</param>
        /// <param name="trackId">The identifier of the track to update.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation.
        /// </returns>
        Task UpdateFile(IFormFile mp3File, string trackId);

        /// <summary>
        /// Deletes the file associated with the specific <paramref name="trackId"/>.
        /// </summary>
        /// <param name="trackId">The identifier of the track to delete.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation, 
        /// with a <c>bool</c> result indicating whether the file was successfully deleted.
        /// </returns>
        Task<bool> DeleteFile(string trackId);
    }

    public class GoogleDriveAppConnectorService : IGoogleDriveAppConnectorService
    {
        /// <summary>
        /// The grpc client for sends/process data from grpc server.
        /// </summary>
        private readonly GoogleDriveConnector.GoogleDriveConnectorClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleDriveAppConnectorService"/> class.
        /// </summary>
        /// <param name="client">The grpc client for sends/process data from grpc server.</param>
        public GoogleDriveAppConnectorService(GoogleDriveConnector.GoogleDriveConnectorClient client)
        {
            this.client = client;
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

        /// <summary>
        /// Converts mp3 file <paramref name="formFile"/> to <see cref="ByteString"/>.
        /// </summary>
        /// <param name="formFile">The mp3 music file to convert.</param>
        /// <returns>
        /// The <see cref="ByteString"/> representing the converted <paramref name="formFile"/>.
        /// </returns>
        private ByteString ConvertToByteString(IFormFile formFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                formFile.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                return ByteString.CopyFrom(fileBytes);
            }
        }
    }
}