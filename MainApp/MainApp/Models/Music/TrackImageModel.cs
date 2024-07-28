namespace MainApp.Models.Music
{
    public class TrackImageModel
    {
        public string ContentType { get; set; } = null!;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
    }
}
