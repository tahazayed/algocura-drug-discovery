using Algocura.Screening.Api.Data;
using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Screening.Api.Messaging.Consumers;

public sealed class CompoundEventsConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<CompoundEventsConsumer> _logger;

    public CompoundEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> options,
        ILogger<CompoundEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _processor = serviceBusClient.CreateProcessor(
            options.Value.TopicName,
            options.Value.SubscriptionName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 4,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
            });
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<ScreeningDbContext>();

            var eventType =
                args.Message.ApplicationProperties
                    .TryGetValue("eventType", out var value)
                    ? value?.ToString()
                    : args.Message.Subject;

            switch (eventType)
            {
                case nameof(CompoundRegistered):
                    {
                        var integrationEvent =
                            args.Message.Body
                                .ToObjectFromJson<CompoundRegistered>(
                                    JsonOptions);

                        await HandleAsync(
                            dbContext,
                            integrationEvent,
                            args.CancellationToken);

                        break;
                    }

                case nameof(CompoundUpdated):
                    {
                        var integrationEvent =
                            args.Message.Body
                                .ToObjectFromJson<CompoundUpdated>(
                                    JsonOptions);

                        await HandleAsync(
                            dbContext,
                            integrationEvent,
                            args.CancellationToken);

                        break;
                    }

                case nameof(CompoundDeleted):
                    {
                        var integrationEvent =
                            args.Message.Body
                                .ToObjectFromJson<CompoundDeleted>(
                                    JsonOptions);

                        await HandleAsync(
                            dbContext,
                            integrationEvent,
                            args.CancellationToken);

                        break;
                    }

                case nameof(CompoundStatusChanged):
                    {
                        var integrationEvent =
                            args.Message.Body
                                .ToObjectFromJson<CompoundStatusChanged>(
                                    JsonOptions);

                        await HandleAsync(
                            dbContext,
                            integrationEvent,
                            args.CancellationToken);

                        break;
                    }

                default:
                    _logger.LogDebug(
                        "Ignoring unsupported event type {EventType}",
                        eventType);

                    break;
            }

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Dead-lettering malformed compound event {MessageId}",
                args.Message.MessageId);

            await args.DeadLetterMessageAsync(
                args.Message,
                "InvalidPayload",
                exception.Message,
                args.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process compound event {MessageId}",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken: args.CancellationToken);
        }
    }

    public static async Task HandleAsync(
        ScreeningDbContext dbContext,
        CompoundRegistered? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "CompoundRegistered payload is empty.");
        }

        var entity =
            await dbContext.ScreeningCompounds
                .SingleOrDefaultAsync(
                    compound =>
                        compound.CompoundId ==
                        integrationEvent.CompoundId,
                    cancellationToken);

        if (entity is null)
        {
            entity = new ScreeningCompound
            {
                CompoundId = integrationEvent.CompoundId,
                RegisteredAtUtc =
                    integrationEvent.CreatedAtUtc
            };

            dbContext.ScreeningCompounds.Add(entity);
        }

        entity.Name = integrationEvent.Name.Trim();
        entity.CanonicalSmiles =
            integrationEvent.CanonicalSmiles.Trim();

        entity.InchiKey =
            integrationEvent.InchiKey?
                .Trim()
                .ToUpperInvariant();

        entity.UpdatedAtUtc =
            integrationEvent.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public static async Task HandleAsync(
        ScreeningDbContext dbContext,
        CompoundUpdated? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "CompoundUpdated payload is empty.");
        }

        var entity =
            await dbContext.ScreeningCompounds
                .SingleOrDefaultAsync(
                    compound =>
                        compound.CompoundId ==
                        integrationEvent.CompoundId,
                    cancellationToken);

        if (entity is null)
        {
            entity = new ScreeningCompound
            {
                CompoundId = integrationEvent.CompoundId,
                RegisteredAtUtc =
                    integrationEvent.OccurredAtUtc
            };

            dbContext.ScreeningCompounds.Add(entity);
        }

        entity.Name = integrationEvent.Name.Trim();
        entity.CanonicalSmiles =
            integrationEvent.CanonicalSmiles.Trim();

        entity.InchiKey =
            integrationEvent.InchiKey?
                .Trim()
                .ToUpperInvariant();

        entity.UpdatedAtUtc =
            integrationEvent.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public static async Task HandleAsync(
        ScreeningDbContext dbContext,
        CompoundDeleted? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "CompoundDeleted payload is empty.");
        }

        var entity =
            await dbContext.ScreeningCompounds
                .SingleOrDefaultAsync(
                    compound =>
                        compound.CompoundId ==
                        integrationEvent.CompoundId,
                    cancellationToken);

        if (entity is null)
        {
            return;
        }

        dbContext.ScreeningCompounds.Remove(entity);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public static async Task HandleAsync(
        ScreeningDbContext dbContext,
        CompoundStatusChanged? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "CompoundStatusChanged payload is empty.");
        }

        var entity =
            await dbContext.ScreeningCompounds
                .SingleOrDefaultAsync(
                    compound =>
                        compound.CompoundId ==
                        integrationEvent.CompoundId,
                    cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.UpdatedAtUtc =
            integrationEvent.UpdatedAtUtc;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus consumer error. Entity: {EntityPath}; Source: {ErrorSource}",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(
            cancellationToken);

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _processor
            .DisposeAsync()
            .AsTask()
            .GetAwaiter()
            .GetResult();

        base.Dispose();
    }
}

