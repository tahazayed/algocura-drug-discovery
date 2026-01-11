using Algocura.AIDiscovery.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Data;

public sealed class AIDiscoveryDbContext(
    DbContextOptions<AIDiscoveryDbContext> options)
    : DbContext(options)
{
    public DbSet<CompoundProjection> Compounds => Set<CompoundProjection>();
    public DbSet<ScreeningProjection> Screenings => Set<ScreeningProjection>();
    public DbSet<ScreeningProjectionResult> ScreeningResults =>
        Set<ScreeningProjectionResult>();
    public DbSet<ModelDefinition> ModelDefinitions =>
        Set<ModelDefinition>();
    public DbSet<PredictionJob> PredictionJobs => Set<PredictionJob>();
    public DbSet<PredictionCandidate> PredictionCandidates =>
        Set<PredictionCandidate>();
    public DbSet<PredictionResult> PredictionResults =>
        Set<PredictionResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompoundProjection>(entity =>
        {
            entity.HasKey(x => x.CompoundId);
            entity.Property(x => x.MolecularWeight).HasPrecision(18, 4);
        });

        modelBuilder.Entity<ScreeningProjection>(entity =>
        {
            entity.HasKey(x => x.ScreeningId);
            entity.HasMany(x => x.Results)
                .WithOne(x => x.Screening)
                .HasForeignKey(x => x.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScreeningProjectionResult>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Score).HasPrecision(18, 6);
            entity.HasIndex(x => new { x.ScreeningId, x.CompoundId })
                .IsUnique();
        });

        modelBuilder.Entity<ModelDefinition>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.Name, x.Version }).IsUnique();
            entity.Property(x => x.ScreeningScoreWeight).HasPrecision(18, 6);
            entity.Property(x => x.MolecularWeightWeight).HasPrecision(18, 6);
            entity.Property(x => x.SelectionBonus).HasPrecision(18, 6);
        });

        modelBuilder.Entity<PredictionJob>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.ModelDefinition)
                .WithMany()
                .HasForeignKey(x => x.ModelDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Candidates)
                .WithOne(x => x.PredictionJob)
                .HasForeignKey(x => x.PredictionJobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Results)
                .WithOne(x => x.PredictionJob)
                .HasForeignKey(x => x.PredictionJobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PredictionCandidate>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ScreeningScore).HasPrecision(18, 6);
            entity.Property(x => x.MolecularWeight).HasPrecision(18, 4);
            entity.HasIndex(x => new { x.PredictionJobId, x.CompoundId })
                .IsUnique();
        });

        modelBuilder.Entity<PredictionResult>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PredictionScore).HasPrecision(18, 6);
            entity.HasIndex(x => new { x.PredictionJobId, x.CompoundId })
                .IsUnique();
        });
    }
}
