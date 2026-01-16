using Algocura.Platform.Auditing.Abstractions;
using Algocura.Workflow.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Workflow.Api.Data;

public sealed class WorkflowDbContext(
    DbContextOptions<WorkflowDbContext> options)
    : DbContext(options)
{
    public DbSet<DiscoveryWorkflow> Workflows =>
        Set<DiscoveryWorkflow>();

    public DbSet<WorkflowStageExecution> WorkflowStages =>
        Set<WorkflowStageExecution>();

    public DbSet<WorkflowEventReceipt> WorkflowEventReceipts =>
        Set<WorkflowEventReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscoveryWorkflow>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.DiseaseName);

            entity.HasMany(x => x.Stages)
                .WithOne(x => x.Workflow)
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.EventReceipts)
                .WithOne(x => x.Workflow)
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowStageExecution>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.HasIndex(x => new
            {
                x.WorkflowId,
                x.Stage
            }).IsUnique();
        });

        modelBuilder.Entity<WorkflowEventReceipt>(entity =>
        {
            entity.HasKey(x => x.EventId);

            entity.Property(x => x.EventId)
                .ValueGeneratedNever();

            entity.HasIndex(x => new
            {
                x.WorkflowId,
                x.EventType
            });
        });

        ConfigureAuditableEntities(
            modelBuilder);
    }

    private static void ConfigureAuditableEntities(
        ModelBuilder modelBuilder)
    {
        var auditableEntityTypes =
            modelBuilder.Model
                .GetEntityTypes()
                .Where(
                    entityType =>
                        typeof(IAuditableEntity)
                            .IsAssignableFrom(
                                entityType.ClrType))
                .ToArray();

        foreach (var entityType in auditableEntityTypes)
        {
            var entity =
                modelBuilder.Entity(
                    entityType.ClrType);

            entity.Property<Guid?>(
                    nameof(
                        IAuditableEntity.CreatedByUserId))
                .IsRequired(false);

            entity.Property<Guid?>(
                    nameof(
                        IAuditableEntity.UpdatedByUserId))
                .IsRequired(false);

            entity.Property<DateTimeOffset>(
                    nameof(
                        IAuditableEntity.CreatedAtUtc))
                .IsRequired();

            entity.Property<DateTimeOffset>(
                    nameof(
                        IAuditableEntity.UpdatedAtUtc))
                .IsRequired();

            entity.HasIndex(
                nameof(
                    IAuditableEntity.CreatedByUserId));

            entity.HasIndex(
                nameof(
                    IAuditableEntity.UpdatedByUserId));
        }
    }
}
