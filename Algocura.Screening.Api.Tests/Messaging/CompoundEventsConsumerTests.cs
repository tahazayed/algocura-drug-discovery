using Algocura.Screening.Api.Events;
using Algocura.Screening.Api.Messaging.Consumers;
using Algocura.Screening.Api.Tests.Support;
using FluentAssertions;

namespace Algocura.Screening.Api.Tests.Messaging;

public sealed class CompoundEventsConsumerTests
{
    [Fact]
    public async Task Handle_RegisteredCompound_InsertsLocalProjection()
    {
        var cancellationToken = CancellationToken.None;

        await using var db = TestDb.Create();

        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var integrationEvent = new CompoundRegistered
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = now,
            CompoundId = id,
            Name = "Aspirin",
            CanonicalSmiles = "SMILES",
            InchiKey = "abc",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await CompoundEventsConsumer.HandleAsync(
            db,
            integrationEvent,
            cancellationToken);

        var compound = await db.ScreeningCompounds.FindAsync(
            [id],
            cancellationToken);

        compound.Should().NotBeNull();
        compound!.InchiKey.Should().Be("ABC");
    }

    [Fact]
    public async Task Handle_UpdatedCompound_UpdatesExistingProjection()
    {
        var cancellationToken = CancellationToken.None;

        await using var db = TestDb.Create();

        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var registeredEvent = new CompoundRegistered
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = now,
            CompoundId = id,
            Name = "Old",
            CanonicalSmiles = "S1",
            InchiKey = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await CompoundEventsConsumer.HandleAsync(
            db,
            registeredEvent,
            cancellationToken);

        var updatedEvent = new CompoundUpdated
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = now.AddMinutes(1),
            CompoundId = id,
            Name = "New",
            CanonicalSmiles = "S2",
            InchiKey = null,
            UpdatedAtUtc = now.AddMinutes(1)
        };

        await CompoundEventsConsumer.HandleAsync(
            db,
            updatedEvent,
            cancellationToken);

        var compound = await db.ScreeningCompounds.FindAsync(
            [id],
            cancellationToken);

        compound.Should().NotBeNull();
        compound!.Name.Should().Be("New");
        compound.CanonicalSmiles.Should().Be("S2");
    }
}
