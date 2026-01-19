using Algocura.Platform.Messaging.Options;
using Algocura.Platform.Messaging.Publishing;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Messaging.DependencyInjection;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddAlgocuraPlatformMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ServiceBusOptions>()
            .Bind(
                configuration.GetSection(
                    ServiceBusOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.TopicName),
                "ServiceBus:TopicName is required.")
            .Validate(
                options =>
                    options.UseEmulator
                        ? !string.IsNullOrWhiteSpace(
                            options.ConnectionString)
                        : !string.IsNullOrWhiteSpace(
                            options.FullyQualifiedNamespace),
                "ServiceBus:ConnectionString is required for the emulator, " +
                "or ServiceBus:FullyQualifiedNamespace is required for Azure.")
            .ValidateOnStart();

        services.AddSingleton<ServiceBusClient>(
            serviceProvider =>
            {
                var options =
                    serviceProvider
                        .GetRequiredService<
                            IOptions<ServiceBusOptions>>()
                        .Value;

                return options.UseEmulator
                    ? new ServiceBusClient(
                        options.ConnectionString)
                    : new ServiceBusClient(
                        options.FullyQualifiedNamespace,
                        new DefaultAzureCredential());
            });

        services.AddSingleton<
            IPlatformMessagePublisher,
            ServiceBusMessagePublisher>();

        services.AddScoped<
            ServiceBusAuditEventPublisher>();

        return services;
    }
}
