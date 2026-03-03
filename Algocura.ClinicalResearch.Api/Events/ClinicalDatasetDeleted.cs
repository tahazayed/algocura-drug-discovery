namespace Algocura.ClinicalResearch.Api.Events;

public sealed record ClinicalDatasetDeleted : IntegrationEvent
{
    public required Guid ClinicalDatasetId { get; init; }
    public required string Name { get; init; }
}
