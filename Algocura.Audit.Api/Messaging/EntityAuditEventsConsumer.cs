using Algocura.Audit.Api.Services;
using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Audit.Api.Messaging;

public sealed class EntityAuditEventsConsumer
    : ServiceBusConsumerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive =
                true
        };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EntityAuditEventsConsumer> _logger;

    public EntityAuditEventsConsumer(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> serviceBusOptions,
        IServiceScopeFactory scopeFactory,
        ILogger<EntityAuditEventsConsumer> logger)
        : base(
            serviceBusClient,
            serviceBusOptions,
            logger)
    {
        _scopeFactory =
            scopeFactory;

        _logger =
            logger;
    }

    protected override async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        var eventType =
            ResolveEventType(
                args.Message);

        if (!string.Equals(
                eventType,
                "EntityAuditEvent",
                StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug(
                "Ignoring unsupported event type {EventType}.",
                eventType);

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);

            return;
        }

        try
        {
            var integrationEvent =
                args.Message.Body
                    .ToObjectFromJson<
                        IncomingEntityAuditEvent>(
                        JsonOptions);

            if (integrationEvent is null)
            {
                throw new JsonException(
                    "EntityAuditEvent payload is empty.");
            }

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var ingestionService =
                scope.ServiceProvider
                    .GetRequiredService<
                        IAuditIngestionService>();

            var inserted =
                await ingestionService.IngestAsync(
                    integrationEvent,
                    args.CancellationToken);

            if (inserted)
            {
                _logger.LogInformation(
                    "Processed audit event {EventId} from " +
                    "{ServiceName} for {EntityType} {EntityId}.",
                    integrationEvent.EventId,
                    integrationEvent.ServiceName,
                    integrationEvent.EntityType,
                    integrationEvent.EntityId);
            }
            else
            {
                _logger.LogInformation(
                    "Audit event {EventId} was already processed.",
                    integrationEvent.EventId);
            }

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Invalid EntityAuditEvent payload. " +
                "Dead-lettering message {MessageId}.",
                args.Message.MessageId);

            await args.DeadLetterMessageAsync(
                args.Message,
                "InvalidAuditPayload",
                exception.Message,
                args.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process audit message {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken:
                    args.CancellationToken);
        }
    }
}
