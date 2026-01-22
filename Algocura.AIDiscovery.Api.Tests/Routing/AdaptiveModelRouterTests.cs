using Algocura.AIDiscovery.Api.Domain;
using Algocura.AIDiscovery.Api.Routing;
using FluentAssertions;

namespace Algocura.AIDiscovery.Api.Tests.Routing;

public sealed class AdaptiveModelRouterTests
{
    [Fact]
    public void Route_SelectsHigherConfidenceSpecialistWhenBudgetAllows()
    {
        var fast = Model("Fast");
        var specialist = Model("Specialist");

        var options = Microsoft.Extensions.Options.Options.Create(
            new ModelRoutingOptions
            {
                Models =
                [
                    Profile(fast.Id, 0.65m, 0.50m, 0.10m),
                    Profile(specialist.Id, 0.90m, 0.90m, 0.30m)
                ]
            });

        var sut = new AdaptiveModelRouter(options);

        var result = sut.Route(new ModelRoutingContext(
            [Candidate(200m)], [fast, specialist], [], 1m));

        result.SelectedModelDefinitionId.Should().Be(specialist.Id);
    }

    [Fact]
    public void Route_ExcludesPreviouslyExecutedModel()
    {
        var first = Model("First");
        var second = Model("Second");

        var options = Microsoft.Extensions.Options.Options.Create(
            new ModelRoutingOptions
            {
                Models =
                [
                    Profile(first.Id, 0.90m, 0.90m, 0.20m),
                    Profile(second.Id, 0.70m, 0.60m, 0.20m)
                ]
            });

        var sut = new AdaptiveModelRouter(options);

        var result = sut.Route(new ModelRoutingContext(
            [Candidate(300m)], [first, second],
            [new PreviousModelExecution(first.Id, 0.50m, 0.20m)], 1m));

        result.SelectedModelDefinitionId.Should().Be(second.Id);
    }

    private static ModelDefinition Model(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Version = "1.0",
        Status = ModelStatus.Active,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    private static PredictionCandidate Candidate(decimal mw) => new()
    {
        Id = Guid.NewGuid(),
        CompoundId = Guid.NewGuid(),
        CompoundName = "Candidate",
        MolecularWeight = mw
    };

    private static ModelRoutingProfile Profile(
        Guid id, decimal confidence, decimal specialization, decimal cost) => new()
        {
            ModelDefinitionId = id,
            PreferredMolecularWeight = 250m,
            MolecularWeightTolerance = 250m,
            ExpectedConfidence = confidence,
            SpecializationScore = specialization,
            EstimatedResourceCost = cost
        };
}
