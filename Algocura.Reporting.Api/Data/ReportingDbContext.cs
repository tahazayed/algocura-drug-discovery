using Algocura.Platform.Auditing.Abstractions;
using Algocura.Reporting.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Reporting.Api.Data;

public sealed class ReportingDbContext(DbContextOptions<ReportingDbContext> options) : DbContext(options)
{
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportRun> ReportRuns => Set<ReportRun>();
    public DbSet<ReportMetric> ReportMetrics => Set<ReportMetric>();
    public DbSet<ReportingProjection> ReportingProjections => Set<ReportingProjection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => x.Name).IsUnique();
            e.HasIndex(x => x.Status);
            e.HasMany(x => x.Runs).WithOne(x => x.ReportDefinition)
                .HasForeignKey(x => x.ReportDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportRun>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => x.Status);
            e.HasMany(x => x.Metrics).WithOne(x => x.ReportRun)
                .HasForeignKey(x => x.ReportRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReportMetric>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Value).HasPrecision(18, 6);
            e.HasIndex(x => new { x.ReportRunId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<ReportingProjection>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Score).HasPrecision(18, 6);
            e.HasIndex(x => new { x.SourceService, x.SourceEntityId, x.ProjectionType }).IsUnique();
            e.HasIndex(x => x.ProjectionType);
            e.HasIndex(x => x.DiseaseName);
            e.HasIndex(x => x.GeneSymbol);
            e.HasIndex(x => x.CompoundName);
            e.HasIndex(x => x.WorkflowId);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(x => typeof(IAuditableEntity).IsAssignableFrom(x.ClrType)))
        {
            var e = modelBuilder.Entity(entityType.ClrType);
            e.Property<Guid?>(nameof(IAuditableEntity.CreatedByUserId)).IsRequired(false);
            e.Property<Guid?>(nameof(IAuditableEntity.UpdatedByUserId)).IsRequired(false);
            e.Property<DateTimeOffset>(nameof(IAuditableEntity.CreatedAtUtc)).IsRequired();
            e.Property<DateTimeOffset>(nameof(IAuditableEntity.UpdatedAtUtc)).IsRequired();
            e.HasIndex(nameof(IAuditableEntity.CreatedByUserId));
            e.HasIndex(nameof(IAuditableEntity.UpdatedByUserId));
        }
    }
}
