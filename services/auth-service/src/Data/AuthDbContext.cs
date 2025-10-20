using Microsoft.EntityFrameworkCore;
using AuthService.Models;

namespace AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        // Initialize with null-forgiving to satisfy nullable reference checks
        public DbSet<User> Users { get; set; } = null!;
    }
}