﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainApp.Models.Music
{
    public class MusicTrack
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRequired]
        public string Title { get; set; } = null!;
        [BsonRequired]
        public Style Style { get; set; } = null!;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public string CreatorId { get; set; } = null!;
    }
}