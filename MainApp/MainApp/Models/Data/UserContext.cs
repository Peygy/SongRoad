using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Models
{
    // Database context for interaction between
    // the crew and user tables in the database and the application
    public class UserContext : IdentityDbContext<UserModel>
    {
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; } = null!;

        public UserContext(DbContextOptions<UserContext> options) : base(options) 
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "User", NormalizedName = "User".ToUpper() },
                new IdentityRole { Name = "Moderator", NormalizedName = "Moderator".ToUpper() },
                new IdentityRole { Name = "Admin", NormalizedName = "Admin".ToUpper() }
            );
        }
    }
}
