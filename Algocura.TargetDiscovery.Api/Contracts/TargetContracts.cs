using Algocura.TargetDiscovery.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.TargetDiscovery.Api.Contracts;

public sealed record CreateTargetAssessmentRequest(
    Guid TargetHypothesisId,
    [property: Required, MaxLength(200)] string Name,
    [property: MaxLength(2000)] string? ScientificRationale,
    decimal GeneticEvidenceScore,
    decimal BiologicalPlausibilityScore,
    decimal DruggabilityScore,
    decimal SafetyRiskScore,
    decimal TranslationalPotentialScore,
    [property: MaxLength(1000)] string? ReviewNotes);

public sealed record UpdateTargetAssessmentRequest(
    decimal GeneticEvidenceScore,
    decimal BiologicalPlausibilityScore,
    decimal DruggabilityScore,
    decimal SafetyRiskScore,
    decimal TranslationalPotentialScore,
    [property: MaxLength(2000)] string? ScientificRationale,
    [property: MaxLength(1000)] string? ReviewNotes);

public sealed record TargetHypothesisResponse(
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string DesiredModulation,
    string Rationale,
    decimal LiteratureEvidenceScore,
    TargetHypothesisProjectionStatus Status,
    DateTimeOffset SourceUpdatedAtUtc,
    DateTimeOffset SynchronizedAtUtc,
    IReadOnlyCollection<Guid> EvidenceStatementIds);

public sealed record TargetAssessmentResponse(
    Guid Id,
    Guid TargetHypothesisId,
    string Name,
    string? ScientificRationale,
    decimal LiteratureEvidenceScore,
    decimal GeneticEvidenceScore,
    decimal BiologicalPlausibilityScore,
    decimal DruggabilityScore,
    decimal SafetyRiskScore,
    decimal TranslationalPotentialScore,
    decimal OverallScore,
    TargetConfidenceLevel ConfidenceLevel,
    TargetAssessmentStatus Status,
    string? ReviewNotes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset? SelectedAtUtc);
