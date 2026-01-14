using Algocura.Notification.Api.Domain;
using Algocura.Notification.Api.Services;
using FluentAssertions;

namespace Algocura.Notification.Api.Tests.Services;

public sealed class NotificationComposerTests
{
    [Fact]
    public void Compose_ReplacesTokensAndCreatesDelivery()
    {
        var rule = new NotificationRule
        {
            Id = Guid.NewGuid(),
            Name = "Rule",
            EventType = "AssayCompleted",
            Channel = NotificationChannel.InApplication,
            Recipient = "scientists",
            SubjectTemplate = "{{eventType}} from {{source}}",
            BodyTemplate = "Event {{eventId}} correlation {{correlationId}}",
            Priority = NotificationPriority.High,
            Enabled = true,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        var envelope = new PlatformEventEnvelope(
            Guid.NewGuid(),
            "AssayCompleted",
            "Algocura.AssayManagement.Api",
            "corr-1",
            DateTimeOffset.UtcNow,
            "{}");

        var result = new DefaultNotificationComposer()
            .Compose(rule, envelope);

        result.Subject.Should()
            .Be("AssayCompleted from Algocura.AssayManagement.Api");
        result.Body.Should().Contain("corr-1");
        result.Deliveries.Should().ContainSingle();
    }
}
