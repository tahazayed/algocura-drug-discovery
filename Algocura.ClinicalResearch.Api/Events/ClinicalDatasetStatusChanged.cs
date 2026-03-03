using Algocura.ClinicalResearch.Api.Domain;

namespace Algocura.ClinicalResearch.Api.Events;

public sealed record ClinicalDatasetStatusChanged : IntegrationEvent
{
    public required Guid ClinicalDatasetId { get; init; }
    public required ClinicalDatasetStatus PreviousStatus { get; init; }
    public required ClinicalDatasetStatus CurrentStatus { get; init; }
}
