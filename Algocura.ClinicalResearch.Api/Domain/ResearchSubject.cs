using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.ClinicalResearch.Api.Domain;

public sealed class ResearchSubject : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ClinicalDatasetId { get; set; }

    [MaxLength(160)]
    public required string ExternalSubjectId { get; set; }

    public int? AgeAtDiagnosis { get; set; }

    [MaxLength(40)]
    public string? Sex { get; set; }

    [MaxLength(200)]
    public string? PrimarySite { get; set; }

    [MaxLength(120)]
    public string? DiseaseStage { get; set; }

    [MaxLength(120)]
    public string? VitalStatus { get; set; }

    [MaxLength(500)]
    public string? Diagnosis { get; set; }

    [MaxLength(2000)]
    public string? TreatmentSummary { get; set; }

    [MaxLength(8000)]
    public string? MolecularFeaturesJson { get; set; }

    [MaxLength(8000)]
    public string? AdditionalAttributesJson { get; set; }

    public DateTimeOffset ImportedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }

    public ClinicalDataset ClinicalDataset { get; set; } = null!;
}
