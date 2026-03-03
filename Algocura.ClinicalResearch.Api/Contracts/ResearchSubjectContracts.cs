using System.ComponentModel.DataAnnotations;

namespace Algocura.ClinicalResearch.Api.Contracts;

public sealed record ResearchSubjectResponse(
    Guid Id,
    Guid ClinicalDatasetId,
    string ExternalSubjectId,
    int? AgeAtDiagnosis,
    string? Sex,
    string? PrimarySite,
    string? DiseaseStage,
    string? VitalStatus,
    string? Diagnosis,
    string? TreatmentSummary,
    string? MolecularFeaturesJson,
    string? AdditionalAttributesJson,
    DateTimeOffset ImportedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ImportResearchSubjectsRequest
{
    [Required, MinLength(1), MaxLength(5000)]
    public required IReadOnlyCollection<ResearchSubjectImportItem> Subjects { get; init; }
}

public sealed record ResearchSubjectImportItem
{
    [Required, StringLength(160, MinimumLength = 1)]
    public required string ExternalSubjectId { get; init; }

    [Range(0, 130)]
    public int? AgeAtDiagnosis { get; init; }

    [StringLength(40)]
    public string? Sex { get; init; }

    [StringLength(200)]
    public string? PrimarySite { get; init; }

    [StringLength(120)]
    public string? DiseaseStage { get; init; }

    [StringLength(120)]
    public string? VitalStatus { get; init; }

    [StringLength(500)]
    public string? Diagnosis { get; init; }

    [StringLength(2000)]
    public string? TreatmentSummary { get; init; }

    [StringLength(8000)]
    public string? MolecularFeaturesJson { get; init; }

    [StringLength(8000)]
    public string? AdditionalAttributesJson { get; init; }
}

public sealed record ImportResearchSubjectsResponse(
    Guid ClinicalDatasetId,
    int InsertedCount,
    int UpdatedCount,
    int TotalSubjectCount,
    DateTimeOffset ImportedAtUtc);
