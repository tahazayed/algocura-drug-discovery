using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Algocura.Screening.Api.Data;
using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Screening.Api.Messaging.Consumers;

public sealed class CompoundEventsConsumer
    : ServiceBusConsumerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CompoundEventsConsumer> _logger;

    public CompoundEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<CompoundEventsConsumer> logger)
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
        try
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        ScreeningDbContext>();

            var eventType =
                ResolveEventType(
                    args.Message);

            switch (eventType)
            {
                case nameof(CompoundRegistered):
                    {
                        var integrationEvent =
                            args.Message.Body
                                .ToObjectFromJson<
                                    CompoundRegistered>(
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
                                .ToObjectFromJson<
                                    CompoundUpdated>(
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
                                .ToObjectFromJson<
                                    CompoundDeleted>(
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
                                .ToObjectFromJson<
                                    CompoundStatusChanged>(
                                    JsonOptions);

                        await HandleAsync(
                            dbContext,
                            integrationEvent,
                            args.CancellationToken);

                        break;
                    }

                default:
                    _logger.LogDebug(
                        "Ignoring unsupported event type {EventType}.",
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
                "Dead-lettering malformed compound event {MessageId}.",
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
                "Failed to process compound event {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken:
                    args.CancellationToken);
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
            entity =
                new ScreeningCompound
                {
                    CompoundId =
                        integrationEvent.CompoundId,

                    RegisteredAtUtc =
                        integrationEvent.CreatedAtUtc
                };

            dbContext.ScreeningCompounds.Add(
                entity);
        }

        entity.Name =
            integrationEvent.Name.Trim();

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
            entity =
                new ScreeningCompound
                {
                    CompoundId =
                        integrationEvent.CompoundId,

                    RegisteredAtUtc =
                        integrationEvent.OccurredAtUtc
                };

            dbContext.ScreeningCompounds.Add(
                entity);
        }

        entity.Name =
            integrationEvent.Name.Trim();

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

        dbContext.ScreeningCompounds.Remove(
            entity);

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
}
