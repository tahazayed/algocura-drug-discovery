using Algocura.TargetDiscovery.Api.Domain;
using Algocura.TargetDiscovery.Api.Services;
using FluentAssertions;

namespace Algocura.TargetDiscovery.Api.Tests.Services;

public sealed class TargetAssessmentScoringEngineTests
{
    [Fact]
    public void Calculate_StrongTarget_ReturnsHighScore()
    {
        var assessment = new TargetAssessment
        {
            Id = Guid.NewGuid(),
            TargetHypothesisId = Guid.NewGuid(),
            Name = "LRRK2 assessment",
            LiteratureEvidenceScore = 0.90m,
            GeneticEvidenceScore = 0.90m,
            BiologicalPlausibilityScore = 0.85m,
            DruggabilityScore = 0.80m,
            SafetyRiskScore = 0.20m,
            TranslationalPotentialScore = 0.80m,
            Status = TargetAssessmentStatus.InProgress,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var sut =
            new DeterministicTargetAssessmentScoringEngine();

        var result =
            sut.Calculate(assessment);

        result.OverallScore.Should()
            .BeGreaterThanOrEqualTo(0.80m);

        result.ConfidenceLevel.Should()
            .BeOneOf(
                TargetConfidenceLevel.High,
                TargetConfidenceLevel.VeryHigh);
    }

    [Fact]
    public void Calculate_InvalidScore_Throws()
    {
        var assessment = new TargetAssessment
        {
            Id = Guid.NewGuid(),
            TargetHypothesisId = Guid.NewGuid(),
            Name = "Invalid",
            LiteratureEvidenceScore = 1.20m,
            GeneticEvidenceScore = 0.50m,
            BiologicalPlausibilityScore = 0.50m,
            DruggabilityScore = 0.50m,
            SafetyRiskScore = 0.50m,
            TranslationalPotentialScore = 0.50m,
            Status = TargetAssessmentStatus.Draft,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var sut =
            new DeterministicTargetAssessmentScoringEngine();

        var action = () =>
            sut.Calculate(assessment);

        action.Should()
            .Throw<ArgumentOutOfRangeException>();
    }
}
