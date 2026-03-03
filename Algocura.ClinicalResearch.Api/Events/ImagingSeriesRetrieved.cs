namespace Algocura.ClinicalResearch.Api.Events;

public sealed record ImagingSeriesRetrieved : IntegrationEvent
{
    public required string Collection { get; init; }
    public required string PatientId { get; init; }
    public required int RetrievedCount { get; init; }
    public required int CuratedCount { get; init; }
}
