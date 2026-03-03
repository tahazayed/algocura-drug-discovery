namespace Algocura.ClinicalResearch.Api.Events;

public sealed record ImmunotherapyResponseEstimated : IntegrationEvent
{
    public string? CancerType { get; init; }
    public string? SeriesInstanceUid { get; init; }
    public required double ResearchResponseScore { get; init; }
    public required string ModelVersion { get; init; }
}
