namespace MainApp.DTO.Music
{
    public class NewMusicTrackModelDTO
    {
        public string Title { get; set; } = null!;
        public string Style { get; set; } = null!;
        public IFormFile Mp3File { get; set; } = null!;
    }
}
