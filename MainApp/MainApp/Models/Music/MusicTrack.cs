﻿using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace MainApp.Models.Music
{
    /// <summary>
    /// Model of music track
    /// </summary>
    public class MusicTrack
    {
        [Key]
        public ObjectId Id { get; set; }
        [Required]
        public string Title { get; set; } = null!;

        // Music style, e.x rock, jazz
        [Required]
        public ObjectId StyleId { get; set; }
        public Style Style { get; set; } = null!;

        public ObjectId TrackImageId { get; set; }
        public TrackImageModel? TrackImage { get; set; }

        [Required]
        public string CreatorId { get; set; } = null!;
        public MusicAuthor Creator { get; set; } = null!;
        public List<MusicAuthor> LikedBy { get; set; } = new List<MusicAuthor>();

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}