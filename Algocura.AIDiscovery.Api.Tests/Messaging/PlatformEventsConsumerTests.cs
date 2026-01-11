#pragma warning disable xUnit1051

using Algocura.AIDiscovery.Api.Events;
using Algocura.AIDiscovery.Api.Messaging.Consumers;
using Algocura.AIDiscovery.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Tests.Messaging;

public sealed class PlatformEventsConsumerTests
{
    [Fact]
    public async Task Handle_CompoundRegistered_InsertsProjection()
    {
        await using var database =
            TestDatabase.Create();

        var now = DateTimeOffset.UtcNow;
        var compoundId = Guid.NewGuid();

        var integrationEvent =
            new CompoundRegistered
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now,
                CompoundId = compoundId,
                Name = "Aspirin",
                CanonicalSmiles =
                    "CC(=O)OC1=CC=CC=C1C(=O)O",
                InchiKey =
                    "BSYNRYMUTXBXSQ-UHFFFAOYSA-N",
                MolecularWeight = 180.16m,
                MolecularFormula = "C9H8O4",
                Status = 2,
                UpdatedAtUtc = now
            };

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            integrationEvent,
            CancellationToken.None);

        var projection =
            await database.Context.Compounds
                .SingleAsync(
                    compound =>
                        compound.CompoundId ==
                        compoundId);

        projection.Name.Should().Be("Aspirin");

        projection.InchiKey.Should()
            .Be("BSYNRYMUTXBXSQ-UHFFFAOYSA-N");

        projection.MolecularWeight.Should()
            .Be(180.16m);
    }

    [Fact]
    public async Task Handle_CompoundRegistered_ReplayedEventUpdatesExistingProjection()
    {
        await using var database =
            TestDatabase.Create();

        var compoundId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new CompoundRegistered
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now,
                CompoundId = compoundId,
                Name = "Original Name",
                CanonicalSmiles = "ORIGINAL",
                InchiKey = "original",
                MolecularWeight = 100m,
                MolecularFormula = "C1H1",
                Status = 1,
                UpdatedAtUtc = now
            },
            CancellationToken.None);

        database.Context.ChangeTracker.Clear();

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new CompoundRegistered
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now.AddMinutes(1),
                CompoundId = compoundId,
                Name = "Updated Name",
                CanonicalSmiles = "UPDATED",
                InchiKey = "updated",
                MolecularWeight = 200m,
                MolecularFormula = "C2H2",
                Status = 2,
                UpdatedAtUtc = now.AddMinutes(1)
            },
            CancellationToken.None);

        var projections =
            await database.Context.Compounds
                .Where(
                    compound =>
                        compound.CompoundId ==
                        compoundId)
                .ToListAsync();

        projections.Should().ContainSingle();

        projections[0].Name.Should()
            .Be("Updated Name");

        projections[0].CanonicalSmiles.Should()
            .Be("UPDATED");

        projections[0].InchiKey.Should()
            .Be("UPDATED");
    }

    [Fact]
    public async Task Handle_CompletedScreeningSnapshot_InsertsScreeningProjection()
    {
        await using var database =
            TestDatabase.Create();

        var now = DateTimeOffset.UtcNow;
        var screeningId = Guid.NewGuid();
        var firstCompoundId = Guid.NewGuid();
        var secondCompoundId = Guid.NewGuid();

        var integrationEvent =
            new CompletedScreeningSnapshot
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now,
                ScreeningId = screeningId,
                Name = "Primary Docking Screening",
                ScreeningMethod = "Molecular Docking",
                CompletedAtUtc = now,
                Results =
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = firstCompoundId,
                        Score = 92.50m,
                        Rank = 1,
                        Selected = true
                    },

                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = secondCompoundId,
                        Score = 81.25m,
                        Rank = 2,
                        Selected = false
                    }
                ]
            };

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            integrationEvent,
            CancellationToken.None);

        var projection =
            await database.Context.Screenings
                .Include(screening =>
                    screening.Results)
                .SingleAsync(
                    screening =>
                        screening.ScreeningId ==
                        screeningId);

        projection.Name.Should()
            .Be("Primary Docking Screening");

        projection.ScreeningMethod.Should()
            .Be("Molecular Docking");

        projection.Results.Should()
            .HaveCount(2);

        projection.Results.Should()
            .ContainSingle(
                result =>
                    result.CompoundId ==
                        firstCompoundId &&
                    result.Score == 92.50m &&
                    result.Rank == 1 &&
                    result.Selected);
    }

    [Fact]
    public async Task Handle_CompletedScreeningSnapshot_ReplayReplacesResultsWithoutDuplicates()
    {
        await using var database =
            TestDatabase.Create();

        var screeningId = Guid.NewGuid();
        var firstCompoundId = Guid.NewGuid();
        var secondCompoundId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var originalEvent =
            new CompletedScreeningSnapshot
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now,
                ScreeningId = screeningId,
                Name = "Original Screening",
                ScreeningMethod = "Docking",
                CompletedAtUtc = now,
                Results =
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = firstCompoundId,
                        Score = 70m,
                        Rank = 1,
                        Selected = false
                    }
                ]
            };

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            originalEvent,
            CancellationToken.None);

        database.Context.ChangeTracker.Clear();

        var replayedEvent =
            new CompletedScreeningSnapshot
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now.AddMinutes(5),
                ScreeningId = screeningId,
                Name = "Updated Screening",
                ScreeningMethod = "Updated Docking",
                CompletedAtUtc = now.AddMinutes(5),
                Results =
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = firstCompoundId,
                        Score = 95m,
                        Rank = 1,
                        Selected = true
                    },

                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = secondCompoundId,
                        Score = 82m,
                        Rank = 2,
                        Selected = false
                    }
                ]
            };

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            replayedEvent,
            CancellationToken.None);

        database.Context.ChangeTracker.Clear();

        var screenings =
            await database.Context.Screenings
                .Include(screening =>
                    screening.Results)
                .Where(
                    screening =>
                        screening.ScreeningId ==
                        screeningId)
                .ToListAsync();

        screenings.Should().ContainSingle();

        var projection = screenings[0];

        projection.Name.Should()
            .Be("Updated Screening");

        projection.ScreeningMethod.Should()
            .Be("Updated Docking");

        projection.Results.Should()
            .HaveCount(2);

        projection.Results.Should()
            .ContainSingle(
                result =>
                    result.CompoundId ==
                        firstCompoundId &&
                    result.Score == 95m &&
                    result.Selected);

        projection.Results.Should()
            .ContainSingle(
                result =>
                    result.CompoundId ==
                        secondCompoundId &&
                    result.Score == 82m);
    }

    [Fact]
    public async Task Handle_CompletedScreeningSnapshot_WithEmptyResults_ClearsExistingResults()
    {
        await using var database =
            TestDatabase.Create();

        var screeningId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new CompletedScreeningSnapshot
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now,
                ScreeningId = screeningId,
                Name = "Screening",
                ScreeningMethod = "Docking",
                CompletedAtUtc = now,
                Results =
                [
                    new CompletedScreeningSnapshotResult
                    {
                        CompoundId = Guid.NewGuid(),
                        Score = 90m,
                        Rank = 1,
                        Selected = true
                    }
                ]
            },
            CancellationToken.None);

        database.Context.ChangeTracker.Clear();

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new CompletedScreeningSnapshot
            {
                EventId = Guid.NewGuid(),
                OccurredAtUtc = now.AddMinutes(1),
                ScreeningId = screeningId,
                Name = "Screening",
                ScreeningMethod = "Docking",
                CompletedAtUtc = now.AddMinutes(1),
                Results = []
            },
            CancellationToken.None);

        database.Context.ChangeTracker.Clear();

        var projection =
            await database.Context.Screenings
                .Include(screening =>
                    screening.Results)
                .SingleAsync(
                    screening =>
                        screening.ScreeningId ==
                        screeningId);

        projection.Results.Should().BeEmpty();
    }
}