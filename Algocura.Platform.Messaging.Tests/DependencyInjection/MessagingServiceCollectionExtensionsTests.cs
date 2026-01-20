using Algocura.Platform.Messaging.DependencyInjection;
using Algocura.Platform.Messaging.Options;
using Algocura.Platform.Messaging.Publishing;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Messaging.Tests.DependencyInjection;

public sealed class MessagingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAlgocuraPlatformMessaging_RegistersSharedServices()
    {
        var configuration =
            CreateConfiguration();

        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddAlgocuraPlatformMessaging(
            configuration);

        services.Should()
            .Contain(
                descriptor =>
                    descriptor.ServiceType ==
                    typeof(ServiceBusClient) &&
                    descriptor.Lifetime ==
                    ServiceLifetime.Singleton);

        services.Should()
            .Contain(
                descriptor =>
                    descriptor.ServiceType ==
                    typeof(IPlatformMessagePublisher) &&
                    descriptor.ImplementationType ==
                    typeof(ServiceBusMessagePublisher) &&
                    descriptor.Lifetime ==
                    ServiceLifetime.Singleton);

        services.Should()
            .Contain(
                descriptor =>
                    descriptor.ServiceType ==
                    typeof(IEventPublisher) &&
                    descriptor.ImplementationType ==
                    typeof(ServiceBusEventPublisher) &&
                    descriptor.Lifetime ==
                    ServiceLifetime.Singleton);

        services.Should()
            .Contain(
                descriptor =>
                    descriptor.ServiceType ==
                    typeof(ServiceBusAuditEventPublisher) &&
                    descriptor.Lifetime ==
                    ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddAlgocuraPlatformMessaging_BindsServiceBusOptions()
    {
        var configuration =
            CreateConfiguration();

        var services =
            new ServiceCollection();

        services.AddLogging();

        services.AddAlgocuraPlatformMessaging(
            configuration);

        using var serviceProvider =
            services.BuildServiceProvider();

        var options =
            serviceProvider
                .GetRequiredService<
                    IOptions<ServiceBusOptions>>()
                .Value;

        options.UseEmulator
            .Should()
            .BeTrue();

        options.TopicName
            .Should()
            .Be("algocura-test-events");

        options.SubscriptionName
            .Should()
            .Be("messaging-tests");

        options.DefaultMessageTimeToLiveDays
            .Should()
            .Be(3);

        options.MaxConcurrentCalls
            .Should()
            .Be(2);

        options.PrefetchCount
            .Should()
            .Be(8);

        options.MaxAutoLockRenewalMinutes
            .Should()
            .Be(4);
    }

    private static IConfiguration CreateConfiguration()
    {
        var values =
            new Dictionary<string, string?>
            {
                ["ServiceBus:UseEmulator"] =
                    "true",

                ["ServiceBus:ConnectionString"] =
                    "Endpoint=sb://localhost/;" +
                    "SharedAccessKeyName=RootManageSharedAccessKey;" +
                    "SharedAccessKey=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=;" +
                    "UseDevelopmentEmulator=true;",

                ["ServiceBus:TopicName"] =
                    "algocura-test-events",

                ["ServiceBus:SubscriptionName"] =
                    "messaging-tests",

                ["ServiceBus:DefaultMessageTimeToLiveDays"] =
                    "3",

                ["ServiceBus:MaxConcurrentCalls"] =
                    "2",

                ["ServiceBus:PrefetchCount"] =
                    "8",

                ["ServiceBus:MaxAutoLockRenewalMinutes"] =
                    "4"
            };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(
                values)
            .Build();
    }
}
