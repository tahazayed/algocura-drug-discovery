using Algocura.ClinicalResearch.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ClinicalResearch.Api.Data;

public sealed class ClinicalResearchDbContext(DbContextOptions<ClinicalResearchDbContext> options)
    : DbContext(options)
{
    public DbSet<ClinicalDataset> ClinicalDatasets => Set<ClinicalDataset>();
    public DbSet<ResearchSubject> ResearchSubjects => Set<ResearchSubject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dataset = modelBuilder.Entity<ClinicalDataset>();
        dataset.ToTable("ClinicalDatasets");
        dataset.HasKey(x => x.Id);
        dataset.Property(x => x.Name).HasMaxLength(160).IsRequired();
        dataset.Property(x => x.SourceName).HasMaxLength(120).IsRequired();
        dataset.Property(x => x.SourceUri).HasMaxLength(1000);
        dataset.Property(x => x.StudyCode).HasMaxLength(100);
        dataset.Property(x => x.DiseaseArea).HasMaxLength(160);
        dataset.Property(x => x.Description).HasMaxLength(2000);
        dataset.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        dataset.Property(x => x.CreatedAtUtc).HasPrecision(0);
        dataset.Property(x => x.UpdatedAtUtc).HasPrecision(0);
        dataset.HasIndex(x => x.Name);
        dataset.HasIndex(x => x.StudyCode);
        dataset.HasIndex(x => x.Status);

        var subject = modelBuilder.Entity<ResearchSubject>();
        subject.ToTable("ResearchSubjects");
        subject.HasKey(x => x.Id);
        subject.Property(x => x.ExternalSubjectId).HasMaxLength(160).IsRequired();
        subject.Property(x => x.Sex).HasMaxLength(40);
        subject.Property(x => x.PrimarySite).HasMaxLength(200);
        subject.Property(x => x.DiseaseStage).HasMaxLength(120);
        subject.Property(x => x.VitalStatus).HasMaxLength(120);
        subject.Property(x => x.Diagnosis).HasMaxLength(500);
        subject.Property(x => x.TreatmentSummary).HasMaxLength(2000);
        subject.Property(x => x.MolecularFeaturesJson).HasMaxLength(8000);
        subject.Property(x => x.AdditionalAttributesJson).HasMaxLength(8000);
        subject.Property(x => x.ImportedAtUtc).HasPrecision(0);
        subject.Property(x => x.CreatedAtUtc).HasPrecision(0);
        subject.Property(x => x.UpdatedAtUtc).HasPrecision(0);

        subject.HasIndex(x => new { x.ClinicalDatasetId, x.ExternalSubjectId }).IsUnique();
        subject.HasIndex(x => x.PrimarySite);
        subject.HasIndex(x => x.DiseaseStage);

        subject.HasOne(x => x.ClinicalDataset)
            .WithMany(x => x.Subjects)
            .HasForeignKey(x => x.ClinicalDatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
