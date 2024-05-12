namespace MainApp.Models.Service
{
    public interface IMusicService
    {
        Task AddUserTrack(string title, string style, IFormFile mp3File, string userId);
    }
}
