namespace Algocura.LiteratureIntelligence.Api.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : Algocura.Platform.Messaging.Events.IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.LiteratureIntelligence.Api";
}

public sealed record PublicationRegistered(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PublicationId,
    string Title,
    string? ExternalId)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(PublicationRegistered);
}

public sealed record LiteratureEvidenceDiscovered(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid EvidenceStatementId,
    Guid PublicationId,
    string DiseaseName,
    string GeneSymbol,
    string RelationshipType,
    decimal ConfidenceScore)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(LiteratureEvidenceDiscovered);
}

public sealed record TargetHypothesisProposed(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string DesiredModulation,
    decimal EvidenceScore)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(TargetHypothesisProposed);
}

public sealed record LiteratureIntelligenceSnapshot(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string DesiredModulation,
    string Rationale,
    decimal EvidenceScore,
    IReadOnlyCollection<Guid> EvidenceStatementIds)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(LiteratureIntelligenceSnapshot);
}
