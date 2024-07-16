using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using NAudio.Wave;
using NAudio.Lame;
using MainApp.Services.Music;

namespace MainApp.Services
{
    public interface IGoogleDriveApi
    {
        Task UploadFile(IFormFile mp3File, string trackId);
        Task<Stream?> DownloadFile(string trackId);
        Task UpdateFile(IFormFile mp3File, string trackId);
        Task<bool> DeleteFile(string trackId);
    }

    /// <summary>
    /// Service for actions with google drive api
    /// </summary>
    public class GoogleDriveApi : IGoogleDriveApi
    {
        private readonly IConfiguration configuration;
        private readonly ITracksCachingService cachingService;
        private readonly ILogger<GoogleDriveApi> log;

        public GoogleDriveApi(IConfiguration configuration, ITracksCachingService cachingService, ILogger<GoogleDriveApi> log)
        {
            this.configuration = configuration;
            this.cachingService = cachingService;
            this.log = log;
        }

        /// <summary>
        /// Method for compress mp3 file
        /// </summary>
        /// <param name="mp3File">Music file model</param>
        /// <returns>Stream with compressed music file</returns>
        private async Task<Stream> CompressMp3FileAsync(IFormFile mp3File)
        {
            var outputStream = new MemoryStream();

            await using (var sourceStream = mp3File.OpenReadStream())
            await using(var reader = new Mp3FileReader(sourceStream))
            {
                using (var writer = new LameMP3FileWriter(outputStream, reader.WaveFormat, LAMEPreset.ABR_128))
                {
                    await reader.CopyToAsync(writer);
                }
            }

            outputStream.Position = 0;
            return outputStream;
        }

        private DriveService InitializeDriveService()
        {
            var credentialPath = configuration.GetSection("GoogleDrive:Credentials").Value;
            using var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.ScopeConstants.DriveFile);

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "songroad"
            });
        }

        /// <summary>
        /// Method for add music track file to google drive
        /// </summary>
        /// <param name="mp3File">Music track file</param>
        /// <param name="trackId">Music track identification number</param>
        /// <returns>Task object</returns>
        public async Task UploadFile(IFormFile mp3File, string trackId)
        {
            var service = InitializeDriveService();
            var folderId = configuration.GetSection("GoogleDrive:Folder").Value;

            var fileMetaData = new Google.Apis.Drive.v3.Data.File
            {
                Name = trackId,
                Parents = new List<string> { folderId }
            };

            await using var fsSource = await CompressMp3FileAsync(mp3File);
            var createRequest = service.Files.Create(fileMetaData, fsSource, "");
            createRequest.Fields = "*";
            var results = await createRequest.UploadAsync(CancellationToken.None);

            if (results.Status == UploadStatus.Failed)
            {
                log.LogError($"Ошибка при загрузке файла: {results.Exception.Message}");
            }
            else
            {
                log.LogInformation($"Файл {trackId} загружен на облако");
            }
        }

        /// <summary>
        /// Method for get file stream of music file on cloud
        /// </summary>
        /// <param name="trackId">Id of music track - music file name</param>
        /// <returns>File stream</returns>
        public async Task<Stream?> DownloadFile(string trackId)
        {
            var service = InitializeDriveService();
            var folderId = configuration.GetSection("GoogleDrive:Folder").Value;

            var request = service.Files.List();
            request.Q = $"parents in '{folderId}'";
            var response = await request.ExecuteAsync();

            var downloadFile = response.Files.FirstOrDefault(file => file.Name == trackId);
            if (downloadFile == null)
            {
                log.LogError($"Файл с названием {trackId} не найден");
                return null;
            }

            var getRequest = service.Files.Get(downloadFile.Id);
            var memoryStream = new MemoryStream();
            await getRequest.DownloadAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        /// <summary>
        /// Method for update music filee on cloud
        /// </summary>
        /// <param name="mp3File">Music file</param>
        /// <param name="trackId">Id of music track - music file name</param>
        /// <returns>Task object</returns>
        public async Task UpdateFile(IFormFile mp3File, string trackId)
        {
            var service = InitializeDriveService();
            var folderId = configuration.GetSection("GoogleDrive:Folder").Value;

            var request = service.Files.List();
            request.Q = $"name = '{trackId}' and trashed = false";
            request.Fields = "files(id, name)";
            var result = await request.ExecuteAsync();

            var file = result.Files.FirstOrDefault();
            if (file == null)
            {
                log.LogError($"Файл с названием {trackId} не найден");
                return;
            }

            var fileMetaData = new Google.Apis.Drive.v3.Data.File { Name = trackId };

            await using var fsSource = await CompressMp3FileAsync(mp3File);
            var updateRequest = service.Files.Update(fileMetaData, file.Id, fsSource, "");
            updateRequest.AddParents = folderId;
            var results = await updateRequest.UploadAsync(CancellationToken.None);

            if (results.Status == UploadStatus.Failed)
            {
                log.LogError($"Ошибка при обновлении файла: {results.Exception.Message}");
            }
            else
            {
                log.LogInformation($"Файл {trackId} обновлен на облаке");
            }
        }

        /// <summary>
        /// Method for delete music track from cloud
        /// </summary>
        /// <param name="trackId">Deleting music track id</param>
        /// <returns>Result of deleting in boolean</returns>
        public async Task<bool> DeleteFile(string trackId)
        {
            var service = InitializeDriveService();

            var request = service.Files.List();
            request.Q = $"name = '{trackId}' and trashed = false";
            request.Fields = "files(id, name)";
            var result = await request.ExecuteAsync();

            var file = result.Files.FirstOrDefault();
            if (file == null)
            {
                log.LogError($"Файл с названием {trackId} не найден");
                return false;
            }

            try
            {
                var deleteRequest = service.Files.Delete(file.Id);
                await deleteRequest.ExecuteAsync();
                log.LogInformation($"Файл {trackId} удален с облака");
                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Ошибка при удалении файла: {ex.Message}");
                return false;
            }
        }
    }
}
