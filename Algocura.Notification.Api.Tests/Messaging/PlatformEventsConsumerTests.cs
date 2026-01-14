using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Messaging.Consumers;
using Algocura.Notification.Api.Services;
using Algocura.Notification.Api.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Notification.Api.Tests.Messaging;

public sealed class PlatformEventsConsumerTests
{
    [Fact]
    public async Task Handle_MatchingRule_CreatesNotification()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        database.Context.NotificationRules.Add(CreateRule());
        await database.Context.SaveChangesAsync(timeout.Token);
        database.Context.ChangeTracker.Clear();

        var eventId = Guid.NewGuid();

        var count = await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new DefaultNotificationComposer(),
            new PlatformEventEnvelope(
                eventId,
                "AssayCompleted",
                "Algocura.AssayManagement.Api",
                "corr-1",
                DateTimeOffset.UtcNow,
                "{}"),
            timeout.Token);

        count.Should().Be(1);

        var notification = await database.Context.Notifications
            .Include(x => x.Deliveries)
            .SingleAsync(x => x.SourceEventId == eventId, timeout.Token);

        notification.Deliveries.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_Replay_DoesNotCreateDuplicate()
    {
        await using var database = TestDatabase.Create();
        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(10));

        database.Context.NotificationRules.Add(CreateRule());
        await database.Context.SaveChangesAsync(timeout.Token);

        var envelope = new PlatformEventEnvelope(
            Guid.NewGuid(),
            "AssayCompleted",
            "Algocura.AssayManagement.Api",
            null,
            DateTimeOffset.UtcNow,
            "{}");

        await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new DefaultNotificationComposer(),
            envelope,
            timeout.Token);

        database.Context.ChangeTracker.Clear();

        var second = await PlatformEventsConsumer.HandleAsync(
            database.Context,
            new DefaultNotificationComposer(),
            envelope,
            timeout.Token);

        second.Should().Be(0);
    }

    private static NotificationRule CreateRule() =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Assay",
            EventType = "AssayCompleted",
            Channel = NotificationChannel.InApplication,
            Recipient = "scientists",
            SubjectTemplate = "Assay completed",
            BodyTemplate = "Event {{eventId}}",
            Priority = NotificationPriority.High,
            Enabled = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
}
