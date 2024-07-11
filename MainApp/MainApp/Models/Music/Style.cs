using MongoDB.Bson;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track style
    /// </summary>
    public class Style
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
