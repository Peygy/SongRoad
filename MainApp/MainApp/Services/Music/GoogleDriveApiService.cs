using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using MainApp.Models.Music;
using System.Collections.Generic;
using static Google.Apis.Requests.BatchRequest;

namespace MainApp.Services
{
    /// <summary>
    /// Service for actions with google drive api
    /// </summary>
    public class GoogleDriveApiService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<GoogleDriveApiService> log;

        public GoogleDriveApiService(IConfiguration configuration, ILogger<GoogleDriveApiService> log)
        {
            this.configuration = configuration;
            this.log = log;
        }

        /// <summary>
        /// Method for add music track file to google drive
        /// </summary>
        /// <param name="mp3File">Music track file</param>
        /// <param name="trackId">Music track identification number</param>
        /// <returns>Task object</returns>
        public async Task UploadMusicFileToGoogleDrive(IFormFile mp3File, string trackId)
        {
            // Path of key to google drive
            var credentialPath = configuration.GetSection("GoogleDrive:Credentials").Value;
            // Folder id on google drive
            var folderId = configuration.GetSection("GoogleDrive:Folder").Value;
            GoogleCredential credential;

            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                // Init credentials for upload file
                credential = GoogleCredential.FromStream(stream).CreateScoped(
                [
                    DriveService.ScopeConstants.DriveFile
                ]);

                // Init service
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "songroad"
                });

                // Create meta data of file
                var fileMetaData = new Google.Apis.Drive.v3.Data.File();
                fileMetaData.Name = trackId;
                fileMetaData.Parents = new List<string>() { folderId };

                // Upload file to google drive
                await using (var fsSource = mp3File.OpenReadStream())
                {
                    var request = service.Files.Create(fileMetaData, fsSource, "");
                    request.Fields = "*";
                    var results = await request.UploadAsync(CancellationToken.None);

                    if (results.Status == UploadStatus.Failed)
                    {
                        log.LogError($"Ошибка при загрузке файла: {results.Exception.Message}");
                    }
                    else
                    {
                        log.LogInformation($"Файл {trackId} загружен на облако");
                    }
                }
            }
        }

        /// <summary>
        /// Method for get file stream of music file on cloud
        /// </summary>
        /// <param name="trackId">Id of music track - music file name</param>
        /// <returns>File stream</returns>
        public async Task<Stream> DownloadMusicFileFromGoogleDrive(string trackId)
        {
            // Path of key to google drive
            var credentialPath = configuration.GetSection("GoogleDrive:Credentials").Value;
            // Folder id on google drive
            var folderId = configuration.GetSection("GoogleDrive:Folder").Value;
            GoogleCredential credential;

            // Init credentials for upload file
            credential = GoogleCredential.FromFile(credentialPath).CreateScoped(
            [
                DriveService.ScopeConstants.Drive
            ]);

            // Init service
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "songroad"
            });

            // Execute request to google drive to get all files
            var request = service.Files.List();
            request.Q = $"parents in '{folderId}'";
            var response = await request.ExecuteAsync();

            // Get file with needed id
            var downloadFile = response.Files.FirstOrDefault(file => file.Name == trackId);
            var getRequest = service.Files.Get(downloadFile.Id);

            // Create stream for using file
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
        public async Task UpdateMusicFileFromGoogleDrive(IFormFile mp3File, string trackId)
        {
            // Path of key to google drive
            var credentialPath = configuration.GetSection("GoogleDrive:Credentials").Value;
            // Folder id on google drive
            var folderId = configuration.GetSection("GoogleDrive:Folder").Value;
            GoogleCredential credential;

            using (var stream = new FileStream(credentialPath, FileMode.Open, FileAccess.Read))
            {
                // Init credentials for upload file
                credential = GoogleCredential.FromStream(stream).CreateScoped(
                [
                    DriveService.ScopeConstants.DriveFile
                ]);

                // Init service
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "songroad"
                });

                // Create meta data of file
                var fileMetaData = new Google.Apis.Drive.v3.Data.File();
                fileMetaData.Name = trackId;

                // Get current file
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

                // Update file to google drive
                await using (var fsSource = mp3File.OpenReadStream())
                {
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
            }
        }
    }
}
