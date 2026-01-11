namespace Algocura.Screening.Api.Events;

public sealed record CompoundRegistered
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid CompoundId { get; init; }

    public required string Name { get; init; }

    public required string CanonicalSmiles { get; init; }

    public string? InchiKey { get; init; }

    public decimal MolecularWeight { get; init; }

    public string? MolecularFormula { get; init; }

    public string? Source { get; init; }

    public string? Description { get; init; }

    public int Status { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record CompoundUpdated
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid CompoundId { get; init; }

    public required string Name { get; init; }

    public required string CanonicalSmiles { get; init; }

    public string? InchiKey { get; init; }

    public decimal MolecularWeight { get; init; }

    public string? MolecularFormula { get; init; }

    public string? Source { get; init; }

    public string? Description { get; init; }

    public int Status { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}

public sealed record CompoundDeleted
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid CompoundId { get; init; }
}

public sealed record CompoundStatusChanged
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid CompoundId { get; init; }

    public int PreviousStatus { get; init; }

    public int CurrentStatus { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }
}
