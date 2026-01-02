using Algocura.CompoundCatalog.Api.Domain;

namespace Algocura.CompoundCatalog.Api.Contracts;

public sealed record CompoundResponse(
    Guid Id,
    string Name,
    string CanonicalSmiles,
    string? InchiKey,
    decimal MolecularWeight,
    string? MolecularFormula,
    string? Source,
    string? Description,
    CompoundStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);