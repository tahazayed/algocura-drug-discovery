using Algocura.LiteratureIntelligence.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Data;

public sealed class LiteratureIntelligenceDbContext(
    DbContextOptions<LiteratureIntelligenceDbContext> options)
    : DbContext(options)
{
    public DbSet<Publication> Publications =>
        Set<Publication>();

    public DbSet<PublicationAuthor> PublicationAuthors =>
        Set<PublicationAuthor>();

    public DbSet<EvidenceStatement> EvidenceStatements =>
        Set<EvidenceStatement>();

    public DbSet<TargetHypothesis> TargetHypotheses =>
        Set<TargetHypothesis>();

    public DbSet<TargetHypothesisEvidence>
        TargetHypothesisEvidence =>
            Set<TargetHypothesisEvidence>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Publication>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.HasIndex(x => x.ExternalId)
                .IsUnique()
                .HasFilter("[ExternalId] IS NOT NULL");

            entity.HasMany(x => x.Authors)
                .WithOne(x => x.Publication)
                .HasForeignKey(x => x.PublicationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.EvidenceStatements)
                .WithOne(x => x.Publication)
                .HasForeignKey(x => x.PublicationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PublicationAuthor>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.HasIndex(x => new
            {
                x.PublicationId,
                x.DisplayOrder
            })
                .IsUnique();
        });

        modelBuilder.Entity<EvidenceStatement>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.ConfidenceScore)
                .HasPrecision(5, 4);

            entity.HasIndex(x => x.DiseaseName);

            entity.HasIndex(x => x.GeneSymbol);
        });

        modelBuilder.Entity<TargetHypothesis>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id)
                .ValueGeneratedNever();

            entity.Property(x => x.EvidenceScore)
                .HasPrecision(5, 4);
        });

        modelBuilder.Entity<TargetHypothesisEvidence>(entity =>
        {
            entity.HasKey(x => new
            {
                x.TargetHypothesisId,
                x.EvidenceStatementId
            });

            entity.HasOne(x => x.TargetHypothesis)
                .WithMany(x => x.EvidenceLinks)
                .HasForeignKey(x => x.TargetHypothesisId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.EvidenceStatement)
                .WithMany()
                .HasForeignKey(x => x.EvidenceStatementId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}