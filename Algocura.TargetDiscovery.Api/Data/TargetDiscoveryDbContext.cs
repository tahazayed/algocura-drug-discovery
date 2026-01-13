using Algocura.TargetDiscovery.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Data;

public sealed class TargetDiscoveryDbContext(
    DbContextOptions<TargetDiscoveryDbContext> options)
    : DbContext(options)
{
    public DbSet<TargetHypothesisProjection> TargetHypotheses =>
        Set<TargetHypothesisProjection>();

    public DbSet<TargetHypothesisEvidenceProjection> TargetHypothesisEvidence =>
        Set<TargetHypothesisEvidenceProjection>();

    public DbSet<TargetAssessment> TargetAssessments =>
        Set<TargetAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TargetHypothesisProjection>(entity =>
        {
            entity.HasKey(x => x.TargetHypothesisId);

            entity.Property(x => x.TargetHypothesisId)
                .ValueGeneratedNever();

            entity.Property(x => x.LiteratureEvidenceScore)
                .HasPrecision(5, 4);

            entity.HasIndex(x => x.DiseaseName);
            entity.HasIndex(x => x.GeneSymbol);

            entity.HasMany(x => x.EvidenceLinks)
                .WithOne(x => x.TargetHypothesis)
                .HasForeignKey(x => x.TargetHypothesisId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Assessments)
                .WithOne(x => x.TargetHypothesis)
                .HasForeignKey(x => x.TargetHypothesisId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TargetHypothesisEvidenceProjection>(entity =>
        {
            entity.HasKey(x => new
            {
                x.TargetHypothesisId,
                x.EvidenceStatementId
            });
        });

        modelBuilder.Entity<TargetAssessment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.LiteratureEvidenceScore)
                .HasPrecision(5, 4);

            entity.Property(x => x.GeneticEvidenceScore)
                .HasPrecision(5, 4);

            entity.Property(x => x.BiologicalPlausibilityScore)
                .HasPrecision(5, 4);

            entity.Property(x => x.DruggabilityScore)
                .HasPrecision(5, 4);

            entity.Property(x => x.SafetyRiskScore)
                .HasPrecision(5, 4);

            entity.Property(x => x.TranslationalPotentialScore)
                .HasPrecision(5, 4);

            entity.Property(x => x.OverallScore)
                .HasPrecision(5, 4);

            entity.HasIndex(x => x.TargetHypothesisId);
            entity.HasIndex(x => x.Status);
        });
    }
}
