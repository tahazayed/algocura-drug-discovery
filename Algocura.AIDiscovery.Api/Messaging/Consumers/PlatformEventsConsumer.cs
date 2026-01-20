using Algocura.AIDiscovery.Api.Data;
using Algocura.AIDiscovery.Api.Domain;
using Algocura.AIDiscovery.Api.Events;
using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.AIDiscovery.Api.Messaging.Consumers;

public sealed class PlatformEventsConsumer
    : ServiceBusConsumerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformEventsConsumer> _logger;

    public PlatformEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<PlatformEventsConsumer> logger)
        : base(
            client,
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
            await using var scope = _scopeFactory.CreateAsyncScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AIDiscoveryDbContext>();

            var eventType =
                ResolveEventType(
                    args.Message);

            switch (eventType)
            {
                case nameof(CompoundRegistered):
                    await HandleAsync(
                        db,
                        args.Message.Body
                            .ToObjectFromJson<CompoundRegistered>(JsonOptions),
                        args.CancellationToken);
                    break;

                case nameof(CompoundUpdated):
                    await HandleAsync(
                        db,
                        args.Message.Body
                            .ToObjectFromJson<CompoundUpdated>(JsonOptions),
                        args.CancellationToken);
                    break;

                case nameof(CompoundDeleted):
                    await HandleAsync(
                        db,
                        args.Message.Body
                            .ToObjectFromJson<CompoundDeleted>(JsonOptions),
                        args.CancellationToken);
                    break;

                case nameof(ScreeningCompleted):
                    await HandleAsync(
                        db,
                        args.Message.Body
                            .ToObjectFromJson<ScreeningCompleted>(JsonOptions),
                        args.CancellationToken);
                    break;
                case nameof(CompletedScreeningSnapshot):
                    await HandleAsync(
                        db,
                        args.Message.Body
                            .ToObjectFromJson<CompletedScreeningSnapshot>(
                                JsonOptions),
                        args.CancellationToken);
                    break;

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
                "Failed to process message {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken: args.CancellationToken);
        }
    }

    public static Task HandleAsync(
    AIDiscoveryDbContext db,
    CompoundRegistered? integrationEvent,
    CancellationToken cancellationToken)
    {
        return HandleCompoundAsync(
            db,
            integrationEvent,
            cancellationToken);
    }

    public static async Task HandleAsync(
        AIDiscoveryDbContext db,
        CompoundUpdated? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "CompoundUpdated payload is empty.");
        }

        var entity = await db.Compounds
            .SingleOrDefaultAsync(
                x => x.CompoundId == integrationEvent.CompoundId,
                cancellationToken);

        if (entity is null)
        {
            entity = new CompoundProjection
            {
                CompoundId = integrationEvent.CompoundId,
                Name = integrationEvent.Name.Trim(),
                CanonicalSmiles =
                    integrationEvent.CanonicalSmiles.Trim()
            };

            db.Compounds.Add(entity);
        }

        entity.Name = integrationEvent.Name.Trim();
        entity.CanonicalSmiles =
            integrationEvent.CanonicalSmiles.Trim();
        entity.InchiKey =
            integrationEvent.InchiKey?
                .Trim()
                .ToUpperInvariant();
        entity.MolecularWeight =
            integrationEvent.MolecularWeight;
        entity.MolecularFormula =
            integrationEvent.MolecularFormula?.Trim();
        entity.Status = integrationEvent.Status;
        entity.UpdatedAtUtc =
            integrationEvent.UpdatedAtUtc;

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task HandleCompoundAsync(
        AIDiscoveryDbContext db,
        CompoundRegistered? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException("Compound payload is empty.");
        }

        var entity = await db.Compounds.SingleOrDefaultAsync(
            x => x.CompoundId == integrationEvent.CompoundId,
            cancellationToken);

        if (entity is null)
        {
            entity = new CompoundProjection
            {
                CompoundId = integrationEvent.CompoundId,
                Name = integrationEvent.Name.Trim(),
                CanonicalSmiles = integrationEvent.CanonicalSmiles.Trim()
            };

            db.Compounds.Add(entity);
        }

        entity.Name = integrationEvent.Name.Trim();
        entity.CanonicalSmiles =
            integrationEvent.CanonicalSmiles.Trim();
        entity.InchiKey =
            integrationEvent.InchiKey?.Trim().ToUpperInvariant();
        entity.MolecularWeight =
            integrationEvent.MolecularWeight;
        entity.MolecularFormula =
            integrationEvent.MolecularFormula?.Trim();
        entity.Status = integrationEvent.Status;
        entity.UpdatedAtUtc =
            integrationEvent.UpdatedAtUtc;

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task HandleAsync(
        AIDiscoveryDbContext db,
        CompoundDeleted? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException("CompoundDeleted payload is empty.");
        }

        var entity = await db.Compounds.SingleOrDefaultAsync(
            x => x.CompoundId == integrationEvent.CompoundId,
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        db.Compounds.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task HandleAsync(
        AIDiscoveryDbContext db,
        ScreeningCompleted? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException("ScreeningCompleted payload is empty.");
        }

        var entity = await db.Screenings
            .Include(x => x.Results)
            .SingleOrDefaultAsync(
                x => x.ScreeningId == integrationEvent.ScreeningId,
                cancellationToken);

        if (entity is null)
        {
            entity = new ScreeningProjection
            {
                ScreeningId = integrationEvent.ScreeningId,
                Name = integrationEvent.Name.Trim(),
                ScreeningMethod = integrationEvent.ScreeningMethod.Trim(),
                CompletedAtUtc = integrationEvent.CompletedAtUtc,
                UpdatedAtUtc = integrationEvent.OccurredAtUtc
            };

            db.Screenings.Add(entity);
        }
        else
        {
            db.ScreeningResults.RemoveRange(entity.Results);
            entity.Results.Clear();
        }

        entity.Name = integrationEvent.Name.Trim();
        entity.ScreeningMethod =
            integrationEvent.ScreeningMethod.Trim();
        entity.CompletedAtUtc =
            integrationEvent.CompletedAtUtc;
        entity.UpdatedAtUtc =
            integrationEvent.OccurredAtUtc;

        foreach (var result in integrationEvent.Results)
        {
            entity.Results.Add(
                new ScreeningProjectionResult
                {
                    Id = Guid.NewGuid(),
                    ScreeningId = integrationEvent.ScreeningId,
                    CompoundId = result.CompoundId,
                    Score = result.Score,
                    Rank = result.Rank,
                    Selected = result.Selected
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }


    public static async Task HandleAsync(
        AIDiscoveryDbContext dbContext,
        CompletedScreeningSnapshot? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "CompletedScreeningSnapshot payload is empty.");
        }

        var screening = await dbContext.Screenings
            .SingleOrDefaultAsync(
                entity =>
                    entity.ScreeningId ==
                    integrationEvent.ScreeningId,
                cancellationToken);

        if (screening is null)
        {
            screening = new ScreeningProjection
            {
                ScreeningId =
                    integrationEvent.ScreeningId,

                Name =
                    integrationEvent.Name.Trim(),

                ScreeningMethod =
                    integrationEvent.ScreeningMethod.Trim(),

                CompletedAtUtc =
                    integrationEvent.CompletedAtUtc,

                UpdatedAtUtc =
                    integrationEvent.OccurredAtUtc
            };

            dbContext.Screenings.Add(screening);
        }
        else
        {
            var existingResults =
                await dbContext.ScreeningResults
                    .Where(
                        result =>
                            result.ScreeningId ==
                            integrationEvent.ScreeningId)
                    .ToListAsync(cancellationToken);

            if (existingResults.Count > 0)
            {
                dbContext.ScreeningResults.RemoveRange(
                    existingResults);

                await dbContext.SaveChangesAsync(
                    cancellationToken);
            }
        }

        screening.Name =
            integrationEvent.Name.Trim();

        screening.ScreeningMethod =
            integrationEvent.ScreeningMethod.Trim();

        screening.CompletedAtUtc =
            integrationEvent.CompletedAtUtc;

        screening.UpdatedAtUtc =
            integrationEvent.OccurredAtUtc;

        var replacementResults =
            integrationEvent.Results
                .Select(
                    result =>
                        new ScreeningProjectionResult
                        {
                            Id = Guid.NewGuid(),

                            ScreeningId =
                                integrationEvent.ScreeningId,

                            CompoundId =
                                result.CompoundId,

                            Score =
                                result.Score,

                            Rank =
                                result.Rank,

                            Selected =
                                result.Selected
                        })
                .ToArray();

        if (replacementResults.Length > 0)
        {
            dbContext.ScreeningResults.AddRange(
                replacementResults);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}