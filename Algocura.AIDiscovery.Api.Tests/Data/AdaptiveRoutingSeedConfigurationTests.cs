using Algocura.AIDiscovery.Api.Data;
using FluentAssertions;

namespace Algocura.AIDiscovery.Api.Tests.Data;

public sealed class AdaptiveRoutingSeedConfigurationTests
{
    [Fact]
    public void SeededAdaptiveModelIds_AreUnique()
    {
        var ids =
            new[]
            {
                AIDiscoverySeeder.PrototypeModelId,
                AIDiscoverySeeder.QsarFastModelId,
                AIDiscoverySeeder.GnnMolecularModelId,
                AIDiscoverySeeder.SmilesTransformerModelId,
                AIDiscoverySeeder.EnsembleConsensusModelId
            };

        ids.Should()
            .OnlyHaveUniqueItems();

        ids.Should()
            .NotContain(
                Guid.Empty);
    }

    [Fact]
    public void SeededAdaptiveModelIds_AreDeterministic()
    {
        AIDiscoverySeeder.QsarFastModelId
            .Should()
            .Be(
                Guid.Parse(
                    "55000000-0000-0000-0000-000000000001"));

        AIDiscoverySeeder.GnnMolecularModelId
            .Should()
            .Be(
                Guid.Parse(
                    "55000000-0000-0000-0000-000000000002"));

        AIDiscoverySeeder.SmilesTransformerModelId
            .Should()
            .Be(
                Guid.Parse(
                    "55000000-0000-0000-0000-000000000003"));

        AIDiscoverySeeder.EnsembleConsensusModelId
            .Should()
            .Be(
                Guid.Parse(
                    "55000000-0000-0000-0000-000000000004"));
    }
}
