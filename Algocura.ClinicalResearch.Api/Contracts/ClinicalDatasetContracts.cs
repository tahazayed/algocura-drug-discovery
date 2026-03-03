using Algocura.ClinicalResearch.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.ClinicalResearch.Api.Contracts;

public sealed record ClinicalDatasetResponse(
    Guid Id,
    string Name,
    string SourceName,
    string? SourceUri,
    string? StudyCode,
    string? DiseaseArea,
    string? Description,
    ClinicalDatasetStatus Status,
    int SubjectCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record CreateClinicalDatasetRequest
{
    [Required, StringLength(160, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required, StringLength(120, MinimumLength = 2)]
    public required string SourceName { get; init; }

    [StringLength(1000)]
    public string? SourceUri { get; init; }

    [StringLength(100)]
    public string? StudyCode { get; init; }

    [StringLength(160)]
    public string? DiseaseArea { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public ClinicalDatasetStatus Status { get; init; } = ClinicalDatasetStatus.Draft;
}

public sealed record UpdateClinicalDatasetRequest
{
    [Required, StringLength(160, MinimumLength = 2)]
    public required string Name { get; init; }

    [Required, StringLength(120, MinimumLength = 2)]
    public required string SourceName { get; init; }

    [StringLength(1000)]
    public string? SourceUri { get; init; }

    [StringLength(100)]
    public string? StudyCode { get; init; }

    [StringLength(160)]
    public string? DiseaseArea { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }

    public ClinicalDatasetStatus Status { get; init; }
}
