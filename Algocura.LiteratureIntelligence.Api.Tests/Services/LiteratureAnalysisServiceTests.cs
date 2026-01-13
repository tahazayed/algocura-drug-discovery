using Algocura.LiteratureIntelligence.Api.Domain;
using Algocura.LiteratureIntelligence.Api.Services;
using FluentAssertions;

namespace Algocura.LiteratureIntelligence.Api.Tests.Services;

public sealed class LiteratureAnalysisServiceTests
{
    [Fact]
    public void Analyze_GeneMentioned_ReturnsEvidence()
    {
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "LRRK2 mutation in Parkinson disease",
            Abstract =
                "LRRK2 mutation is associated with Parkinson disease.",
            Status = PublicationStatus.PendingAnalysis,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var sut = new RuleBasedLiteratureAnalysisService();

        var evidence = sut.Analyze(
            publication,
            "Parkinson disease",
            ["LRRK2"]);

        evidence.Should().ContainSingle();
        evidence.Single().GeneSymbol.Should().Be("LRRK2");
        evidence.Single().HumanValidated.Should().BeFalse();
    }

    [Fact]
    public void Analyze_GeneNotMentioned_ReturnsEmpty()
    {
        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = "Disease biology",
            Abstract = "No requested gene is mentioned.",
            Status = PublicationStatus.PendingAnalysis,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var sut = new RuleBasedLiteratureAnalysisService();

        var evidence = sut.Analyze(
            publication,
            "Disease",
            ["LRRK2"]);

        evidence.Should().BeEmpty();
    }
}
