using MainApp.Models.Music;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace MainApp.Data
{
    // Context for music info database (MongoDb)
    public class MusicContext : DbContext
    {
        public DbSet<MusicTrack> MusicTracks { get; set; }
        public DbSet<Style> Styles { get; set; }
        public DbSet<TrackImageModel> TrackImages { get; set; }
        public DbSet<MusicAuthor> MusicAuthors { get; set; }

        public MusicContext(DbContextOptions options) : base(options)
        {
            SeedStylesAsync().GetAwaiter().GetResult();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MusicTrack>(entity =>
            {
                entity.ToCollection("tracks");

                entity.HasOne(t => t.Creator)
                      .WithMany(a => a.UploadedTracks)
                      .HasForeignKey(t => t.CreatorId);

                entity.HasOne(t => t.Style)
                      .WithMany(s => s.MusicTracks)
                      .HasForeignKey(t => t.StyleId);

                entity.HasOne(t => t.TrackImage)
                      .WithOne(i => i.MusicTrack)
                      .HasForeignKey<MusicTrack>(t => t.TrackImageId);

                entity.HasMany(t => t.LikedBy)
                      .WithMany(a => a.LikedTracks)
                      .UsingEntity(j => j.ToCollection("music_track_likes"));
            });

            modelBuilder.Entity<Style>(entity =>
            {
                entity.ToCollection("styles");
            });

            modelBuilder.Entity<TrackImageModel>(entity =>
            {
                entity.ToCollection("track_images");
            });

            modelBuilder.Entity<MusicAuthor>(entity =>
            {
                entity.ToCollection("music_authors");
            });
        }

        private async Task SeedStylesAsync()
        {
            if (!await Styles.AnyAsync())
            {
                var styles = new List<Style>
                {
                    new Style { Name = "rock" },
                    new Style { Name = "pop" },
                    new Style { Name = "rap" },
                    new Style { Name = "electronic" },
                    new Style { Name = "classical" }
                };

                await Styles.AddRangeAsync(styles);
                await SaveChangesAsync();
            }
        }
    }

    public class MongoDBSettings
    {
        public string ConnectionURL { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}
