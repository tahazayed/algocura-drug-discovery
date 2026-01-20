using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Messaging.Tests.Consumers;

public sealed class ServiceBusConsumerBaseTests
{
    [Fact]
    public void Constructor_WithoutSubscriptionName_Throws()
    {
        var options =
            Microsoft.Extensions.Options.Options.Create(
                new ServiceBusOptions
                {
                    TopicName =
                        "algocura-events",

                    SubscriptionName =
                        null
                });

        var client =
            new TestServiceBusClient();

        var logger =
            new TestLogger<TestConsumer>();

        Action action =
            () =>
                _ = new TestConsumer(
                    client,
                    options,
                    logger);

        action
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "*ServiceBus:SubscriptionName is required*");
    }

    [Fact]
    public void ResolveEventType_UsesApplicationPropertyFirst()
    {
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                messageId:
                    Guid.NewGuid().ToString(),

                subject:
                    "SubjectFallback",

                properties:
                    new Dictionary<string, object>
                    {
                        ["eventType"] =
                            "CompoundUpdated"
                    });

        var eventType =
            TestConsumer.GetEventType(
                message);

        eventType
            .Should()
            .Be("CompoundUpdated");
    }

    [Fact]
    public void ResolveEventType_FallsBackToSubject()
    {
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                messageId:
                    Guid.NewGuid().ToString(),

                subject:
                    "TargetDiscoverySnapshot");

        var eventType =
            TestConsumer.GetEventType(
                message);

        eventType
            .Should()
            .Be("TargetDiscoverySnapshot");
    }

    [Fact]
    public void ResolveSource_UsesSourceApplicationProperty()
    {
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                messageId:
                    Guid.NewGuid().ToString(),

                properties:
                    new Dictionary<string, object>
                    {
                        ["source"] =
                            "Algocura.Screening.Api"
                    });

        var source =
            TestConsumer.GetSource(
                message);

        source
            .Should()
            .Be("Algocura.Screening.Api");
    }

    [Fact]
    public void ResolveSource_WithoutSource_ReturnsUnknown()
    {
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                messageId:
                    Guid.NewGuid().ToString());

        var source =
            TestConsumer.GetSource(
                message);

        source
            .Should()
            .Be("Unknown");
    }

    [Fact]
    public void ResolveEventId_WithGuidMessageId_ReturnsMessageId()
    {
        var eventId =
            Guid.NewGuid();

        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                messageId:
                    eventId.ToString());

        var result =
            TestConsumer.GetEventId(
                message);

        result
            .Should()
            .Be(eventId);
    }

    [Fact]
    public void ResolveEventId_WithInvalidMessageId_GeneratesGuid()
    {
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                messageId:
                    "not-a-guid");

        var result =
            TestConsumer.GetEventId(
                message);

        result
            .Should()
            .NotBe(Guid.Empty);
    }

    private sealed class TestConsumer
        : ServiceBusConsumerBase
    {
        public TestConsumer(
            ServiceBusClient client,
            IOptions<ServiceBusOptions> options,
            ILogger<TestConsumer> logger)
            : base(
                client,
                options,
                logger)
        {
        }

        public static string? GetEventType(
            ServiceBusReceivedMessage message) =>
            ResolveEventType(
                message);

        public static string GetSource(
            ServiceBusReceivedMessage message) =>
            ResolveSource(
                message);

        public static Guid GetEventId(
            ServiceBusReceivedMessage message) =>
            ResolveEventId(
                message);

        protected override Task ProcessMessageAsync(
            ProcessMessageEventArgs args) =>
            Task.CompletedTask;
    }

    private sealed class TestServiceBusClient
        : ServiceBusClient
    {
        public TestServiceBusClient()
        {
        }
    }

    private sealed class TestLogger<T>
        : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(
            TState state)
            where TState : notnull =>
            null;

        public bool IsEnabled(
            LogLevel logLevel) =>
            false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
