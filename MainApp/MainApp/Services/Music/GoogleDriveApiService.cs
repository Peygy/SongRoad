using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;

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
        /// <returns>Task object</returns>
        public async Task UploadMusicFileToGoogleDrive(IFormFile mp3File)
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
                fileMetaData.Name = mp3File.FileName;
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
                        log.LogInformation($"Файл {mp3File.FileName} загружен на облако");
                    }
                }
            }
        }
    }
}
