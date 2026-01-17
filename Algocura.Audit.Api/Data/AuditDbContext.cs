using Algocura.Audit.Api.Domain;
using Algocura.Platform.Auditing.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Data;

public sealed class AuditDbContext(
    DbContextOptions<AuditDbContext> options)
    : DbContext(options)
{
    public DbSet<AuditRecord> AuditRecords =>
        Set<AuditRecord>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditRecord>(entity =>
        {
            entity.ToTable("AuditRecords");

            entity.HasKey(x => x.EventId);

            entity.Property(x => x.ServiceName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.EntityType)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.EntityId)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Action)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Subject)
                .HasMaxLength(500);

            entity.Property(x => x.UserDisplayName)
                .HasMaxLength(300);

            entity.Property(x => x.UserEmail)
                .HasMaxLength(320);

            entity.Property(x => x.ActorType)
                .HasMaxLength(100)
                .IsRequired();

            // Do NOT use HasColumnType("nvarchar(max)").
            // EF will map these appropriately for SQL Server and SQLite.
            entity.Property(x => x.PreviousValuesJson)
            .HasColumnType("nvarchar(max)");

            entity.Property(x => x.CurrentValuesJson)
            .HasColumnType("nvarchar(max)");

            entity.Property(x => x.ChangedPropertiesJson)
            .HasColumnType("nvarchar(max)");

            entity.Property(x => x.CorrelationId)
                .HasMaxLength(200);

            entity.Property(x => x.WorkflowId)
                .HasMaxLength(200);

            entity.Property(x => x.TraceId)
                .HasMaxLength(200);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(1000);

            entity.Property(x => x.EventType)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.EventVersion)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.OccurredAtUtc);

            entity.HasIndex(x => x.ServiceName);

            entity.HasIndex(x => new
            {
                x.EntityType,
                x.EntityId
            });

            entity.HasIndex(x => x.UserId);

            entity.HasIndex(x => x.CorrelationId);

            entity.HasIndex(x => x.WorkflowId);
        });

        ConfigureAuditableEntities(modelBuilder);
    }

    private static void ConfigureAuditableEntities(
        ModelBuilder modelBuilder)
    {
        var auditableEntityTypes =
            modelBuilder.Model
                .GetEntityTypes()
                .Where(entityType =>
                    typeof(IAuditableEntity)
                        .IsAssignableFrom(entityType.ClrType))
                .ToArray();

        foreach (var entityType in auditableEntityTypes)
        {
            var entity =
                modelBuilder.Entity(entityType.ClrType);

            entity.Property<Guid?>(
                    nameof(IAuditableEntity.CreatedByUserId))
                .IsRequired(false);

            entity.Property<Guid?>(
                    nameof(IAuditableEntity.UpdatedByUserId))
                .IsRequired(false);

            entity.Property<DateTimeOffset>(
                    nameof(IAuditableEntity.CreatedAtUtc))
                .IsRequired();

            entity.Property<DateTimeOffset>(
                    nameof(IAuditableEntity.UpdatedAtUtc))
                .IsRequired();

            entity.HasIndex(
                nameof(IAuditableEntity.CreatedByUserId));

            entity.HasIndex(
                nameof(IAuditableEntity.UpdatedByUserId));
        }
    }
}
