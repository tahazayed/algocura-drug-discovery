using Algocura.TargetDiscovery.Api.Contracts;
using Algocura.TargetDiscovery.Api.Domain;

namespace Algocura.TargetDiscovery.Api.Mapping;

public static class TargetMappings
{
    public static TargetHypothesisResponse ToResponse(
        this TargetHypothesisProjection hypothesis)
    {
        return new TargetHypothesisResponse(
            hypothesis.TargetHypothesisId,
            hypothesis.DiseaseName,
            hypothesis.GeneSymbol,
            hypothesis.ProteinTarget,
            hypothesis.DesiredModulation,
            hypothesis.Rationale,
            hypothesis.LiteratureEvidenceScore,
            hypothesis.Status,
            hypothesis.SourceUpdatedAtUtc,
            hypothesis.SynchronizedAtUtc,
            hypothesis.EvidenceLinks
                .Select(x => x.EvidenceStatementId)
                .ToArray());
    }

    public static TargetAssessmentResponse ToResponse(
        this TargetAssessment assessment)
    {
        return new TargetAssessmentResponse(
            assessment.Id,
            assessment.TargetHypothesisId,
            assessment.Name,
            assessment.ScientificRationale,
            assessment.LiteratureEvidenceScore,
            assessment.GeneticEvidenceScore,
            assessment.BiologicalPlausibilityScore,
            assessment.DruggabilityScore,
            assessment.SafetyRiskScore,
            assessment.TranslationalPotentialScore,
            assessment.OverallScore,
            assessment.ConfidenceLevel,
            assessment.Status,
            assessment.ReviewNotes,
            assessment.CreatedAtUtc,
            assessment.UpdatedAtUtc,
            assessment.CompletedAtUtc,
            assessment.SelectedAtUtc);
    }
}
