using Algocura.Platform.Messaging.Events;
using FluentAssertions;

namespace Algocura.Platform.Messaging.Tests.Events;

public sealed class IntegrationEventTests
{
    [Fact]
    public void IntegrationEvent_UsesVersionOneByDefault()
    {
        var integrationEvent =
            new TestIntegrationEvent(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow);

        integrationEvent.EventVersion
            .Should()
            .Be("1.0");
    }

    [Fact]
    public void IntegrationEvent_ExposesRequiredMetadata()
    {
        var eventId =
            Guid.NewGuid();

        var occurredAtUtc =
            new DateTimeOffset(
                2026,
                8,
                9,
                12,
                30,
                0,
                TimeSpan.Zero);

        IIntegrationEvent integrationEvent =
            new TestIntegrationEvent(
                eventId,
                occurredAtUtc);

        integrationEvent.EventId
            .Should()
            .Be(eventId);

        integrationEvent.OccurredAtUtc
            .Should()
            .Be(occurredAtUtc);

        integrationEvent.EventType
            .Should()
            .Be(nameof(TestIntegrationEvent));

        integrationEvent.EventVersion
            .Should()
            .Be("1.0");

        integrationEvent.Source
            .Should()
            .Be("Algocura.Platform.Messaging.Tests");
    }

    [Fact]
    public void IntegrationEvent_AllowsEventVersionOverride()
    {
        var integrationEvent =
            new VersionTwoIntegrationEvent(
                Guid.NewGuid(),
                DateTimeOffset.UtcNow);

        integrationEvent.EventVersion
            .Should()
            .Be("2.0");
    }

    private sealed record TestIntegrationEvent(
        Guid EventId,
        DateTimeOffset OccurredAtUtc)
        : IntegrationEvent(
            EventId,
            OccurredAtUtc)
    {
        public override string EventType =>
            nameof(TestIntegrationEvent);

        public override string Source =>
            "Algocura.Platform.Messaging.Tests";
    }

    private sealed record VersionTwoIntegrationEvent(
        Guid EventId,
        DateTimeOffset OccurredAtUtc)
        : IntegrationEvent(
            EventId,
            OccurredAtUtc)
    {
        public override string EventType =>
            nameof(VersionTwoIntegrationEvent);

        public override string EventVersion =>
            "2.0";

        public override string Source =>
            "Algocura.Platform.Messaging.Tests";
    }
}
