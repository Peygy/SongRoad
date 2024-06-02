namespace MainApp.Data
{
    // Context for music info database (MongoDb)
    public class MusicContext
    {
        public string ConnectionURL { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public List<string> CollectionNames { get; set; } = new List<string>();
    }
}
