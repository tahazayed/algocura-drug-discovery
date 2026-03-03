using System.ComponentModel.DataAnnotations;

namespace Algocura.ClinicalResearch.Api.Contracts;

public sealed record TciaPatientResponse(
    string Collection,
    string PatientId);

public sealed record TciaSeriesResponse(
    string Collection,
    string PatientId,
    string StudyInstanceUid,
    string SeriesInstanceUid,
    string? Modality,
    string? BodyPartExamined,
    string? SeriesDescription,
    int? ImageCount,
    string? StudyDate);

public sealed record CuratedImagingSeriesRequest
{
    [Required, StringLength(200)] public required string Collection { get; init; }
    [Required, StringLength(200)] public required string PatientId { get; init; }
    [StringLength(80)] public string? Modality { get; init; } = "CT";
    [StringLength(200)] public string? BodyPartContains { get; init; }
    public int MinimumImageCount { get; init; } = 1;
}

public sealed record CuratedImagingSeriesResponse(
    int RetrievedCount,
    int CuratedCount,
    IReadOnlyCollection<TciaSeriesResponse> Series);

public sealed record ImmunotherapyResponseEstimateRequest
{
    [Range(0, 130)] public int Age { get; init; }
    [Range(0, 1)] public double NormalizedTumorBurden { get; init; }
    [Range(0, 1)] public double NormalizedPdL1 { get; init; }
    [Range(0, 1)] public double NormalizedTmb { get; init; }
    [Range(0, 1)] public double NormalizedImmuneInfiltration { get; init; }
    [Range(0, 1)] public double PriorTreatmentBurden { get; init; }
    [StringLength(120)] public string? CancerType { get; init; }
    [StringLength(200)] public string? SeriesInstanceUid { get; init; }
}

public sealed record ImmunotherapyResponseEstimateResponse(
    double ResearchResponseScore,
    string ModelName,
    string ModelVersion,
    IReadOnlyDictionary<string, double> FeatureContributions,
    string Disclaimer);

public sealed record SimulatedFollowUpCtResponse(
    string FileName,
    string ContentType,
    double RequestedResponseScore,
    double AppliedShrinkageFraction,
    int OriginalLesionRadiusPixels,
    int SimulatedLesionRadiusPixels,
    string Disclaimer);
