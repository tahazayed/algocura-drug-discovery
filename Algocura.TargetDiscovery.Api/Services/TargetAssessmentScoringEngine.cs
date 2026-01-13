using Algocura.TargetDiscovery.Api.Domain;

namespace Algocura.TargetDiscovery.Api.Services;

public interface ITargetAssessmentScoringEngine
{
    TargetAssessmentScore Calculate(TargetAssessment assessment);
}

public sealed record TargetAssessmentScore(
    decimal OverallScore,
    TargetConfidenceLevel ConfidenceLevel);

public sealed class DeterministicTargetAssessmentScoringEngine
    : ITargetAssessmentScoringEngine
{
    public TargetAssessmentScore Calculate(TargetAssessment assessment)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        ValidateScore(
            assessment.LiteratureEvidenceScore,
            nameof(assessment.LiteratureEvidenceScore));

        ValidateScore(
            assessment.GeneticEvidenceScore,
            nameof(assessment.GeneticEvidenceScore));

        ValidateScore(
            assessment.BiologicalPlausibilityScore,
            nameof(assessment.BiologicalPlausibilityScore));

        ValidateScore(
            assessment.DruggabilityScore,
            nameof(assessment.DruggabilityScore));

        ValidateScore(
            assessment.SafetyRiskScore,
            nameof(assessment.SafetyRiskScore));

        ValidateScore(
            assessment.TranslationalPotentialScore,
            nameof(assessment.TranslationalPotentialScore));

        var safetyContribution =
            1m - assessment.SafetyRiskScore;

        var overallScore =
            assessment.LiteratureEvidenceScore * 0.20m +
            assessment.GeneticEvidenceScore * 0.20m +
            assessment.BiologicalPlausibilityScore * 0.20m +
            assessment.DruggabilityScore * 0.20m +
            safetyContribution * 0.10m +
            assessment.TranslationalPotentialScore * 0.10m;

        overallScore = decimal.Round(
            Math.Clamp(overallScore, 0m, 1m),
            4);

        var confidenceLevel = overallScore switch
        {
            >= 0.85m => TargetConfidenceLevel.VeryHigh,
            >= 0.70m => TargetConfidenceLevel.High,
            >= 0.50m => TargetConfidenceLevel.Moderate,
            _ => TargetConfidenceLevel.Low
        };

        return new TargetAssessmentScore(
            overallScore,
            confidenceLevel);
    }

    private static void ValidateScore(
        decimal value,
        string parameterName)
    {
        if (value is < 0m or > 1m)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Scores must be between 0 and 1.");
        }
    }
}
