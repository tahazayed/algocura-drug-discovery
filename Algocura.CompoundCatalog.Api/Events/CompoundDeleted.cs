namespace Algocura.CompoundCatalog.Api.Events;

public sealed record CompoundDeleted : IntegrationEvent
{
    public required Guid CompoundId { get; init; }

    public required string Name { get; init; }
}