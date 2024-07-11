using MainApp.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MainApp.Data
{
    /// <summary>
    /// Database context for interaction between the crew and user tables in the database and the application
    /// </summary>
    public class UserContext : IdentityDbContext<UserModel>
    {
        public DbSet<RefreshTokenModel> RefreshTokens { get; set; } = null!;
        public DbSet<UserRights> UserRights { get; set; } = null!;

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }
    }
}
