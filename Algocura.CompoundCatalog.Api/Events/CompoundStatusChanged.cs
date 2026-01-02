using Algocura.CompoundCatalog.Api.Domain;

namespace Algocura.CompoundCatalog.Api.Events;

public sealed record CompoundStatusChanged : IntegrationEvent
{
    public required Guid CompoundId { get; init; }

    public required CompoundStatus PreviousStatus { get; init; }

    public required CompoundStatus CurrentStatus { get; init; }

    public string? Reason { get; init; }
}