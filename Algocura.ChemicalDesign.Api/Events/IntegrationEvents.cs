namespace Algocura.ChemicalDesign.Api.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : Algocura.Platform.Messaging.Events.IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.ChemicalDesign.Api";
}

public sealed record TargetDiscoverySnapshot
{
    public Guid EventId { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public Guid TargetAssessmentId { get; init; }
    public Guid TargetHypothesisId { get; init; }
    public required string DiseaseName { get; init; }
    public required string GeneSymbol { get; init; }
    public string? ProteinTarget { get; init; }
    public required string DesiredModulation { get; init; }
    public required string Rationale { get; init; }
    public decimal LiteratureEvidenceScore { get; init; }
    public decimal OverallScore { get; init; }
    public required string ConfidenceLevel { get; init; }
}

public sealed record ChemicalDesignProjectCreated(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid DesignProjectId,
    Guid TargetAssessmentId, string Name, string GeneSymbol)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(ChemicalDesignProjectCreated);
}

public sealed record StructureSearchCompleted(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid StructureSearchJobId,
    Guid DesignProjectId, string SearchType, int CandidateCount)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(StructureSearchCompleted);
}

public sealed record CandidateSelectedForRegistration(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid CandidateStructureId,
    Guid DesignProjectId, Guid TargetAssessmentId, string Name,
    string CanonicalSmiles, string? InchiKey, string? MolecularFormula,
    decimal MolecularWeight, string SelectionRationale)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(CandidateSelectedForRegistration);
}

public sealed record ChemicalDesignSnapshot(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid CandidateStructureId,
    Guid DesignProjectId, Guid TargetAssessmentId, Guid TargetHypothesisId,
    string DiseaseName, string GeneSymbol, string? ProteinTarget,
    string DesiredModulation, string Name, string CanonicalSmiles,
    string? InchiKey, string? MolecularFormula, decimal MolecularWeight,
    decimal MatchScore, string SelectionRationale)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(ChemicalDesignSnapshot);
}
