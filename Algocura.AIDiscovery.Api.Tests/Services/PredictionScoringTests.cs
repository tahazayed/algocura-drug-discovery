using Algocura.AIDiscovery.Api.Domain;
using Algocura.AIDiscovery.Api.Services;
using FluentAssertions;

namespace Algocura.AIDiscovery.Api.Tests.Services;

public sealed class PredictionScoringTests
{
    [Fact]
    public void Score_HigherScreeningScore_RanksFirst()
    {
        var model = new ModelDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Model",
            Version = "1.0",
            ScreeningScoreWeight = 0.8m,
            MolecularWeightWeight = 0.2m,
            SelectionBonus = 0m,
            Status = ModelStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var job = new PredictionJob
        {
            Id = Guid.NewGuid(),
            Name = "Prediction",
            ModelDefinitionId = model.Id,
            Status = PredictionStatus.Running,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Candidates =
            [
                new PredictionCandidate
                {
                    Id = Guid.NewGuid(),
                    CompoundId = Guid.NewGuid(),
                    CompoundName = "A",
                    ScreeningScore = 40m,
                    MolecularWeight = 300m
                },
                new PredictionCandidate
                {
                    Id = Guid.NewGuid(),
                    CompoundId = Guid.NewGuid(),
                    CompoundName = "B",
                    ScreeningScore = 90m,
                    MolecularWeight = 300m
                }
            ]
        };

        var sut = new DeterministicPredictionScoringEngine();

        var results = sut.Score(job, model).ToArray();

        results.Should().HaveCount(2);
        results[0].CompoundId.Should()
            .Be(job.Candidates[1].CompoundId);
        results[0].Rank.Should().Be(1);
    }
}
