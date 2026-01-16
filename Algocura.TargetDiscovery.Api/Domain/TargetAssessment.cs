using System.ComponentModel.DataAnnotations;

namespace Algocura.TargetDiscovery.Api.Domain;

public sealed class TargetAssessment
{
    public Guid Id { get; set; }

    public Guid TargetHypothesisId { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? ScientificRationale { get; set; }

    public decimal LiteratureEvidenceScore { get; set; }

    public decimal GeneticEvidenceScore { get; set; }

    public decimal BiologicalPlausibilityScore { get; set; }

    public decimal DruggabilityScore { get; set; }

    public decimal SafetyRiskScore { get; set; }

    public decimal TranslationalPotentialScore { get; set; }

    public decimal OverallScore { get; set; }

    public TargetConfidenceLevel ConfidenceLevel { get; set; }

    public TargetAssessmentStatus Status { get; set; }

    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public DateTimeOffset? SelectedAtUtc { get; set; }

    public TargetHypothesisProjection? TargetHypothesis { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}
