using Microsoft.EntityFrameworkCore;
using api.Domain.Entities;

namespace api.Data;

public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            entity.Property(x => x.Reference).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.FailureReason).HasMaxLength(250);
        });

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.ToTable("idempotency_keys");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RequestHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.Key).IsUnique();
            entity.HasOne(x => x.Payment)
                .WithMany(x => x.IdempotencyKeys)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WebhookEvent>(entity =>
        {
            entity.ToTable("webhook_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventExternalId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Signature).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(x => x.EventExternalId).IsUnique();
            entity.HasOne(x => x.Payment)
                .WithMany(x => x.WebhookEvents)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
