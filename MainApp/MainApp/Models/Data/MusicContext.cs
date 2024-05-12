namespace MainApp.Models.Data
{
    public class MusicContext
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public List<string> CollectionName { get; set; } = new List<string>();
    }
}
