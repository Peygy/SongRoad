namespace MainApp.DTO.Music
{
    public class MusicTrackModelDTO
    {
        public string Title { get; set; } = null!;
        public string Style { get; set; } = null!;
        public string FileId { get; set; } = null!;
        public string CreationDate { get; set; } = null!;
        public string ImageBase64 { get; set; } = null!;
    }
}
