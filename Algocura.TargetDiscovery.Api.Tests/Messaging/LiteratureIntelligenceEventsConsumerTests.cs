using Algocura.TargetDiscovery.Api.Events;
using Algocura.TargetDiscovery.Api.Messaging.Consumers;
using Algocura.TargetDiscovery.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Tests.Messaging;

public sealed class LiteratureIntelligenceEventsConsumerTests
{
    [Fact]
    public async Task Handle_Snapshot_InsertsHypothesisProjection()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var hypothesisId = Guid.NewGuid();
        var evidenceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await LiteratureIntelligenceEventsConsumer.HandleAsync(
            database.Context,
            new LiteratureIntelligenceSnapshot
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now,
                TargetHypothesisId = hypothesisId,
                DiseaseName = "Parkinson disease",
                GeneSymbol = "LRRK2",
                ProteinTarget =
                    "Leucine-rich repeat kinase 2",
                DesiredModulation = "Inhibit",
                Rationale = "Literature-supported hypothesis.",
                EvidenceScore = 0.82m,
                EvidenceStatementIds = [evidenceId]
            },
            timeout.Token);

        var projection = await database.Context.TargetHypotheses
            .Include(x => x.EvidenceLinks)
            .SingleAsync(
                x => x.TargetHypothesisId == hypothesisId,
                timeout.Token);

        projection.GeneSymbol.Should().Be("LRRK2");
        projection.LiteratureEvidenceScore.Should().Be(0.82m);
        projection.EvidenceLinks.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ReplayedSnapshot_UpdatesWithoutDuplicates()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var hypothesisId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await LiteratureIntelligenceEventsConsumer.HandleAsync(
            database.Context,
            CreateSnapshot(
                hypothesisId,
                "Original rationale",
                0.70m,
                [Guid.NewGuid()]),
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var newEvidenceIds =
            new[] { Guid.NewGuid(), Guid.NewGuid() };

        await LiteratureIntelligenceEventsConsumer.HandleAsync(
            database.Context,
            CreateSnapshot(
                hypothesisId,
                "Updated rationale",
                0.90m,
                newEvidenceIds),
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var projections = await database.Context.TargetHypotheses
            .Include(x => x.EvidenceLinks)
            .Where(
                x => x.TargetHypothesisId == hypothesisId)
            .ToListAsync(timeout.Token);

        projections.Should().ContainSingle();

        projections[0].Rationale.Should()
            .Be("Updated rationale");

        projections[0].LiteratureEvidenceScore.Should()
            .Be(0.90m);

        projections[0].EvidenceLinks.Should()
            .HaveCount(2);
    }

    private static LiteratureIntelligenceSnapshot CreateSnapshot(
        Guid hypothesisId,
        string rationale,
        decimal score,
        IReadOnlyCollection<Guid> evidenceIds)
    {
        return new LiteratureIntelligenceSnapshot
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            TargetHypothesisId = hypothesisId,
            DiseaseName = "Parkinson disease",
            GeneSymbol = "LRRK2",
            ProteinTarget =
                "Leucine-rich repeat kinase 2",
            DesiredModulation = "Inhibit",
            Rationale = rationale,
            EvidenceScore = score,
            EvidenceStatementIds = evidenceIds
        };
    }
}
