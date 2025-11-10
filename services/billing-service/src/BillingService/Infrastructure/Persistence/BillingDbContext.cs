namespace BillingService.Infrastructure.Persistence;

using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Billing service database context
/// </summary>
public sealed class BillingDbContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Invoice
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasMany(e => e.LineItems).WithOne().HasForeignKey("InvoiceId");
            entity.HasMany(e => e.Payments).WithOne().HasForeignKey("InvoiceId");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
        });

        // Configure InvoiceLineItem
        modelBuilder.Entity<InvoiceLineItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        // Configure Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.InvoiceId).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExternalTransactionId).HasMaxLength(100);
            entity.Property(e => e.ExternalPaymentGateway).HasMaxLength(50);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.InvoiceId);
            entity.HasIndex(e => e.Status);
        });

        // Configure Plan
        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.FeaturesJson).HasColumnType("TEXT");
            entity.HasIndex(e => e.IsActive);
        });

        // Configure Subscription
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PlanId).IsRequired();
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.HasOne(e => e.Plan).WithMany().HasForeignKey(e => e.PlanId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Invoices).WithOne().HasForeignKey("SubscriptionId");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextBillingDate);
        });
    }
}
