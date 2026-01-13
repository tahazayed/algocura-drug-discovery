namespace Algocura.TargetDiscovery.Api.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
{
    public abstract string EventType { get; }

    public virtual string EventVersion => "1.0";

    public virtual string Source =>
        "Algocura.TargetDiscovery.Api";
}

// Consumed from Literature Intelligence API.
public sealed record LiteratureIntelligenceSnapshot
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public Guid TargetHypothesisId { get; init; }

    public required string DiseaseName { get; init; }

    public required string GeneSymbol { get; init; }

    public string? ProteinTarget { get; init; }

    public required string DesiredModulation { get; init; }

    public required string Rationale { get; init; }

    public decimal EvidenceScore { get; init; }

    public IReadOnlyCollection<Guid> EvidenceStatementIds { get; init; } = [];
}

public sealed record TargetAssessmentCreated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid TargetAssessmentId,
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(TargetAssessmentCreated);
}

public sealed record TargetAssessmentCompleted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid TargetAssessmentId,
    Guid TargetHypothesisId,
    decimal OverallScore,
    string ConfidenceLevel)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(TargetAssessmentCompleted);
}

public sealed record TargetSelected(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid TargetAssessmentId,
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string DesiredModulation,
    decimal OverallScore)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(TargetSelected);
}

public sealed record TargetDiscoverySnapshot(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid TargetAssessmentId,
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string DesiredModulation,
    string Rationale,
    decimal LiteratureEvidenceScore,
    decimal OverallScore,
    string ConfidenceLevel)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(TargetDiscoverySnapshot);
}
