using Algocura.ChemicalDesign.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Data;

public sealed class ChemicalDesignDbContext(
    DbContextOptions<ChemicalDesignDbContext> options)
    : DbContext(options)
{
    public DbSet<SelectedTargetProjection> SelectedTargets =>
        Set<SelectedTargetProjection>();

    public DbSet<ChemicalDesignProject> DesignProjects =>
        Set<ChemicalDesignProject>();

    public DbSet<QueryStructure> QueryStructures =>
        Set<QueryStructure>();

    public DbSet<StructureSearchJob> StructureSearchJobs =>
        Set<StructureSearchJob>();

    public DbSet<CandidateStructure> CandidateStructures =>
        Set<CandidateStructure>();

    public DbSet<SynthesisRoute> SynthesisRoutes =>
        Set<SynthesisRoute>();

    public DbSet<ReactionStep> ReactionSteps =>
        Set<ReactionStep>();

    public DbSet<ChemicalLibraryEntry> ChemicalLibrary =>
        Set<ChemicalLibraryEntry>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SelectedTargetProjection>(
            entity =>
            {
                entity.HasKey(
                    x => x.TargetAssessmentId);

                entity.Property(
                        x => x.TargetAssessmentId)
                    .ValueGeneratedNever();

                entity.Property(
                        x => x.LiteratureEvidenceScore)
                    .HasPrecision(5, 4);

                entity.Property(
                        x => x.TargetAssessmentScore)
                    .HasPrecision(5, 4);

                entity.HasIndex(
                    x => x.GeneSymbol);

                entity.HasIndex(
                    x => x.DiseaseName);

                entity.HasMany(
                        x => x.DesignProjects)
                    .WithOne(
                        x => x.SelectedTarget)
                    .HasForeignKey(
                        x => x.TargetAssessmentId)
                    .OnDelete(
                        DeleteBehavior.Restrict);
            });

        modelBuilder.Entity<ChemicalDesignProject>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();

                entity.HasIndex(
                    x => x.TargetAssessmentId);

                entity.HasIndex(
                    x => x.Status);

                entity.HasMany(
                        x => x.QueryStructures)
                    .WithOne(
                        x => x.DesignProject)
                    .HasForeignKey(
                        x => x.DesignProjectId)
                    .OnDelete(
                        DeleteBehavior.Cascade);

                entity.HasMany(
                        x => x.SearchJobs)
                    .WithOne(
                        x => x.DesignProject)
                    .HasForeignKey(
                        x => x.DesignProjectId)
                    .OnDelete(
                        DeleteBehavior.Cascade);

                entity.HasMany(
                        x => x.Candidates)
                    .WithOne(
                        x => x.DesignProject)
                    .HasForeignKey(
                        x => x.DesignProjectId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<QueryStructure>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();
            });

        modelBuilder.Entity<StructureSearchJob>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(
                        x => x.SimilarityThreshold)
                    .HasPrecision(5, 4);

                entity.HasOne(
                        x => x.QueryStructure)
                    .WithMany()
                    .HasForeignKey(
                        x => x.QueryStructureId)
                    .OnDelete(
                        DeleteBehavior.Restrict);

                entity.HasMany(
                        x => x.Candidates)
                    .WithOne(
                        x => x.StructureSearchJob)
                    .HasForeignKey(
                        x => x.StructureSearchJobId)
                    .OnDelete(
                        DeleteBehavior.NoAction);
            });

        modelBuilder.Entity<CandidateStructure>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(
                        x => x.MolecularWeight)
                    .HasPrecision(18, 4);

                entity.Property(
                        x => x.MatchScore)
                    .HasPrecision(5, 4);

                entity.HasIndex(
                    x => x.InchiKey);

                entity.HasIndex(
                    x => x.Status);

                entity.HasMany(
                        x => x.SynthesisRoutes)
                    .WithOne(
                        x => x.CandidateStructure)
                    .HasForeignKey(
                        x => x.CandidateStructureId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<SynthesisRoute>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(
                        x => x.EstimatedYieldPercent)
                    .HasPrecision(6, 2);

                entity.HasMany(
                        x => x.Steps)
                    .WithOne(
                        x => x.SynthesisRoute)
                    .HasForeignKey(
                        x => x.SynthesisRouteId)
                    .OnDelete(
                        DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<ReactionStep>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(
                        x => x.EstimatedYieldPercent)
                    .HasPrecision(6, 2);

                entity.HasIndex(
                        x => new
                        {
                            x.SynthesisRouteId,
                            x.StepNumber
                        })
                    .IsUnique();
            });

        modelBuilder.Entity<ChemicalLibraryEntry>(
            entity =>
            {
                entity.HasKey(
                    x => x.Id);

                entity.Property(
                        x => x.Id)
                    .ValueGeneratedNever();

                entity.Property(
                        x => x.MolecularWeight)
                    .HasPrecision(18, 4);

                entity.HasIndex(
                        x => x.InchiKey)
                    .IsUnique()
                    .HasFilter(
                        "[InchiKey] IS NOT NULL");
            });
    }
}