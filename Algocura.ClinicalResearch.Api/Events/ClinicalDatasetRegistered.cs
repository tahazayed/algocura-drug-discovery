using Algocura.ClinicalResearch.Api.Domain;

namespace Algocura.ClinicalResearch.Api.Events;

public sealed record ClinicalDatasetRegistered : IntegrationEvent
{
    public required Guid ClinicalDatasetId { get; init; }
    public required string Name { get; init; }
    public required string SourceName { get; init; }
    public string? SourceUri { get; init; }
    public string? StudyCode { get; init; }
    public string? DiseaseArea { get; init; }
    public ClinicalDatasetStatus Status { get; init; }
}
