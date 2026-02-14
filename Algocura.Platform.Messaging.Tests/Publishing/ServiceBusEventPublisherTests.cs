using Algocura.Platform.Messaging.Events;
using Algocura.Platform.Messaging.Publishing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Algocura.Platform.Messaging.Tests.Publishing;

public sealed class ServiceBusEventPublisherTests
{
    [Fact]
    public async Task PublishAsync_ForwardsEventAndStandardMetadata()
    {
        var platformPublisher =
            Substitute.For<
                IPlatformMessagePublisher>();

        var logger =
            Substitute.For<
                ILogger<ServiceBusEventPublisher>>();

        var sut =
            new ServiceBusEventPublisher(
                platformPublisher,
                logger);

        var eventId =
            Guid.NewGuid();

        var occurredAtUtc =
            new DateTimeOffset(
                2026,
                8,
                9,
                18,
                15,
                0,
                TimeSpan.Zero);

        var correlationId =
            Guid.NewGuid().ToString();

        var integrationEvent =
            new TestIntegrationEvent(
                eventId,
                occurredAtUtc);

        await sut.PublishAsync(
            integrationEvent,
            correlationId);

        await platformPublisher
            .Received(1)
            .PublishAsync(
                integrationEvent,
                nameof(TestIntegrationEvent),
                eventId,
                correlationId,
                Arg.Is<IReadOnlyDictionary<string, object>>(
                      properties =>
        properties != null && properties.Count == 3 &&
        Equals(
            properties["eventVersion"],
            "1.0") &&
        Equals(
            properties["source"],
            "Algocura.Platform.Messaging.Tests") &&
        Equals(
            properties["occurredAtUtc"],
            occurredAtUtc.ToString("O"))),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_Throws()
    {
        var platformPublisher =
            Substitute.For<
                IPlatformMessagePublisher>();

        var logger =
            Substitute.For<
                ILogger<ServiceBusEventPublisher>>();

        var sut =
            new ServiceBusEventPublisher(
                platformPublisher,
                logger);

        Func<Task> action =
            async () =>
                await sut.PublishAsync<TestIntegrationEvent>(
                    null!);

        await action
            .Should()
            .ThrowAsync<ArgumentNullException>();

        await platformPublisher
            .DidNotReceiveWithAnyArgs()
            .PublishAsync<TestIntegrationEvent>(
                default!,
                default!,
                default,
                default,
                default,
                default);
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
}
