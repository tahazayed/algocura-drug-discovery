using Algocura.ChemicalDesign.Api.Events;
using Algocura.ChemicalDesign.Api.Messaging.Consumers;
using Algocura.ChemicalDesign.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Tests.Messaging;

public sealed class TargetDiscoveryEventsConsumerTests
{
    [Fact]
    public async Task Handle_Snapshot_InsertsProjection()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var assessmentId = Guid.NewGuid();

        await TargetDiscoveryEventsConsumer.HandleAsync(
            database.Context,
            Snapshot(assessmentId, 0.84m),
            timeout.Token);

        var entity = await database.Context.SelectedTargets
            .SingleAsync(
                x => x.TargetAssessmentId == assessmentId,
                timeout.Token);

        entity.GeneSymbol.Should().Be("LRRK2");
        entity.TargetAssessmentScore.Should().Be(0.84m);
    }

    [Fact]
    public async Task Handle_Replay_UpdatesWithoutDuplicate()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var assessmentId = Guid.NewGuid();

        await TargetDiscoveryEventsConsumer.HandleAsync(
            database.Context,
            Snapshot(assessmentId, 0.70m),
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        await TargetDiscoveryEventsConsumer.HandleAsync(
            database.Context,
            Snapshot(assessmentId, 0.91m),
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var items = await database.Context.SelectedTargets
            .Where(x => x.TargetAssessmentId == assessmentId)
            .ToListAsync(timeout.Token);

        items.Should().ContainSingle();
        items[0].TargetAssessmentScore.Should().Be(0.91m);
    }

    private static TargetDiscoverySnapshot Snapshot(
        Guid assessmentId,
        decimal score) =>
        new()
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            TargetAssessmentId = assessmentId,
            TargetHypothesisId = Guid.NewGuid(),
            DiseaseName = "Parkinson disease",
            GeneSymbol = "LRRK2",
            ProteinTarget = "Leucine-rich repeat kinase 2",
            DesiredModulation = "Inhibit",
            Rationale = "Validated target",
            LiteratureEvidenceScore = 0.82m,
            OverallScore = score,
            ConfidenceLevel = "High"
        };
}
