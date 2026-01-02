using Algocura.CompoundCatalog.Api.Domain;

namespace Algocura.CompoundCatalog.Api.Events;

public sealed record CompoundRegistered : IntegrationEvent
{
    public required Guid CompoundId { get; init; }

    public required string Name { get; init; }

    public required string CanonicalSmiles { get; init; }

    public string? InchiKey { get; init; }

    public decimal MolecularWeight { get; init; }

    public string? MolecularFormula { get; init; }

    public CompoundStatus Status { get; init; }
}