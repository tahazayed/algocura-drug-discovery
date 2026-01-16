using Algocura.Platform.Auditing.Abstractions;
using Algocura.Screening.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Data;

public sealed class ScreeningDbContext(DbContextOptions<ScreeningDbContext> options) : DbContext(options)
{
    public DbSet<ScreeningJob> ScreeningJobs => Set<ScreeningJob>();
    public DbSet<ScreeningCandidate> ScreeningCandidates => Set<ScreeningCandidate>();
    public DbSet<ScreeningResult> ScreeningResults => Set<ScreeningResult>();
    public DbSet<ScreeningCompound> ScreeningCompounds => Set<ScreeningCompound>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScreeningJob>(entity =>
        {
            entity.ToTable("ScreeningJobs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.ScreeningMethod).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<ScreeningCandidate>(entity =>
        {
            entity.ToTable("ScreeningCandidates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CompoundName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.CanonicalSmiles).HasMaxLength(4000).IsRequired();
            entity.HasIndex(x => new { x.ScreeningJobId, x.CompoundId }).IsUnique();
            entity.HasOne(x => x.ScreeningJob).WithMany(x => x.Candidates).HasForeignKey(x => x.ScreeningJobId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScreeningResult>(entity =>
        {
            entity.ToTable("ScreeningResults");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Score).HasPrecision(18, 8);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasIndex(x => new { x.ScreeningJobId, x.CompoundId }).IsUnique();
            entity.HasOne(x => x.ScreeningJob).WithMany(x => x.Results).HasForeignKey(x => x.ScreeningJobId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScreeningCompound>(entity =>
        {
            entity.ToTable("ScreeningCompounds");
            entity.HasKey(x => x.CompoundId);
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
            entity.Property(x => x.CanonicalSmiles).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.InchiKey).HasMaxLength(27);
            entity.HasIndex(x => x.InchiKey);
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
