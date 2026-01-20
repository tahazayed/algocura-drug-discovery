using Algocura.Platform.Messaging.Options;
using FluentAssertions;

namespace Algocura.Platform.Messaging.Tests.Options;

public sealed class ServiceBusOptionsTests
{
    [Fact]
    public void Defaults_ArePlatformDefaults()
    {
        var options =
            new ServiceBusOptions();

        options.UseEmulator
            .Should()
            .BeFalse();

        options.ConnectionString
            .Should()
            .BeEmpty();

        options.FullyQualifiedNamespace
            .Should()
            .BeEmpty();

        options.TopicName
            .Should()
            .Be("algocura-events");

        options.SubscriptionName
            .Should()
            .BeNull();

        options.DefaultMessageTimeToLiveDays
            .Should()
            .Be(7);

        options.MaxConcurrentCalls
            .Should()
            .Be(4);

        options.PrefetchCount
            .Should()
            .Be(0);

        options.MaxAutoLockRenewalMinutes
            .Should()
            .Be(5);
    }

    [Fact]
    public void SectionName_IsServiceBus()
    {
        ServiceBusOptions.SectionName
            .Should()
            .Be("ServiceBus");
    }
}
