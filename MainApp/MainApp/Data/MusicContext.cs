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
                entity.ToCollection("tracks").HasKey(e => e.Id);
            });

            modelBuilder.Entity<MusicAuthor>(entity =>
            {
                entity.ToCollection("music_authors").HasKey(e => e.Id);
            });

            modelBuilder.Entity<Style>(entity =>
            {
                entity.ToCollection("styles").HasKey(e => e.Id);
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
