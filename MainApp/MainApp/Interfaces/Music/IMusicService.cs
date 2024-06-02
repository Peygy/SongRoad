namespace MainApp.Interfaces.Music
{
    public interface IMusicService
    {
        // Add new uploaded track
        Task AddTrackAsync(string title, string style, IFormFile mp3File, string userId);
    }
}
