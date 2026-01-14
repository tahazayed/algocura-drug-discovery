using Algocura.AssayManagement.Api.Events;
using Algocura.AssayManagement.Api.Messaging.Consumers;
using Algocura.AssayManagement.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Tests.Messaging;

public sealed class ScreeningEventsConsumerTests
{
    [Fact]
    public async Task Handle_Snapshot_InsertsScreeningProjection()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var screeningId = Guid.NewGuid();
        var compoundId = Guid.NewGuid();

        await ScreeningEventsConsumer.HandleAsync(
            database.Context,
            Snapshot(
                screeningId,
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = compoundId,
                        Score = 0.88m,
                        Rank = 1,
                        Selected = true
                    }
                ]),
            timeout.Token);

        var screening = await database.Context.Screenings
            .Include(x => x.Results)
            .SingleAsync(
                x => x.ScreeningId == screeningId,
                timeout.Token);

        screening.Results.Should().ContainSingle();
        screening.Results.Single().CompoundId.Should().Be(compoundId);
    }

    [Fact]
    public async Task Handle_ReplayedSnapshot_ReplacesResults()
    {
        await using var database =
            TestDatabase.Create();

        using var timeout =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

        var screeningId = Guid.NewGuid();

        await ScreeningEventsConsumer.HandleAsync(
            database.Context,
            Snapshot(
                screeningId,
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = Guid.NewGuid(),
                        Score = 0.70m,
                        Rank = 1,
                        Selected = true
                    }
                ]),
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        await ScreeningEventsConsumer.HandleAsync(
            database.Context,
            Snapshot(
                screeningId,
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = Guid.NewGuid(),
                        Score = 0.91m,
                        Rank = 1,
                        Selected = true
                    },
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = Guid.NewGuid(),
                        Score = 0.75m,
                        Rank = 2,
                        Selected = false
                    }
                ]),
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var results = await database.Context.ScreeningResults
            .Where(x => x.ScreeningId == screeningId)
            .ToListAsync(timeout.Token);

        results.Should().HaveCount(2);
    }

    private static CompletedScreeningSnapshot Snapshot(
        Guid screeningId,
        IReadOnlyCollection<CompletedScreeningSnapshotResult> results)
    {
        return new CompletedScreeningSnapshot
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            ScreeningId = screeningId,
            Name = "Screening",
            ScreeningMethod = "Virtual",
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Results = results
        };
    }
}
