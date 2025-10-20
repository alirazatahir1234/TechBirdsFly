using Microsoft.EntityFrameworkCore;
using BillingService.Models;

namespace BillingService.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    public DbSet<BillingAccount> BillingAccounts { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<UsageMetric> UsageMetrics { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // BillingAccount indexes
        modelBuilder.Entity<BillingAccount>()
            .HasIndex(b => b.UserId);

        // Invoice indexes
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.BillingAccountId);

        // UsageMetric indexes
        modelBuilder.Entity<UsageMetric>()
            .HasIndex(u => u.UserId);
        
        modelBuilder.Entity<UsageMetric>()
            .HasIndex(u => u.EventDate);
    }
}
