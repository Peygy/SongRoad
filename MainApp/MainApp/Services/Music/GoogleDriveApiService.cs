namespace MainApp.Services
{
    /// <summary>
    /// Service for actions with google drive api
    /// </summary>
    public class GoogleDriveApiService
    {
        private readonly IConfiguration configuration;

        public GoogleDriveApiService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Method for add music track file to google drive
        /// </summary>
        /// <param name="mp3File">Music track file</param>
        /// <returns>Task object</returns>
        public async Task AddMusicFileToGoogleDrive(IFormFile mp3File)
        {

        }
    }
}
