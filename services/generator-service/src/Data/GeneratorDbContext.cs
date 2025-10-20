using Microsoft.EntityFrameworkCore;
using GeneratorService.Models;

namespace GeneratorService.Data
{
    public class GeneratorDbContext : DbContext
    {
        public GeneratorDbContext(DbContextOptions<GeneratorDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<GenerateWebsiteJob> GenerateWebsiteJobs { get; set; } = null!;
    }
}