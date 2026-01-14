using Algocura.Notification.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Data;

public sealed class NotificationDbContext(
    DbContextOptions<NotificationDbContext> options) : DbContext(options)
{
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<Algocura.Notification.Api.Domain.Notification> Notifications => Set<Algocura.Notification.Api.Domain.Notification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<NotificationDeliveryAttempt> NotificationDeliveryAttempts => Set<NotificationDeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.EventType, x.Enabled });
            e.HasMany(x => x.Notifications).WithOne(x => x.Rule)
                .HasForeignKey(x => x.RuleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Algocura.Notification.Api.Domain.Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => x.SourceEventId);
            e.HasIndex(x => x.Status);
            e.HasMany(x => x.Deliveries).WithOne(x => x.Notification)
                .HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationDelivery>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.NotificationId, x.Channel, x.Recipient }).IsUnique();
            e.HasMany(x => x.Attempts).WithOne(x => x.NotificationDelivery)
                .HasForeignKey(x => x.NotificationDeliveryId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationDeliveryAttempt>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.NotificationDeliveryId, x.AttemptNumber }).IsUnique();
        });
    }
}
