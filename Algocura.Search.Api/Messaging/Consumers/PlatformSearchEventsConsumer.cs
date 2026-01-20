using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Algocura.Platform.Messaging.Publishing;
using Algocura.Search.Api.Data;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Events;
using Algocura.Search.Api.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Search.Api.Messaging.Consumers;

public sealed class PlatformSearchEventsConsumer
    : ServiceBusConsumerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformSearchEventsConsumer> _logger;

    public PlatformSearchEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<PlatformSearchEventsConsumer> logger)
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
            var eventType =
                ResolveEventType(
                    args.Message);

            var source =
                args.Message.ApplicationProperties
                    .TryGetValue(
                        "source",
                        out var sourceValue)
                    ? sourceValue?.ToString()
                    : "Unknown";

            if (string.IsNullOrWhiteSpace(
                    eventType))
            {
                throw new JsonException(
                    "The event type is missing.");
            }

            var eventId =
                Guid.TryParse(
                    args.Message.MessageId,
                    out var parsedEventId)
                    ? parsedEventId
                    : Guid.NewGuid();

            var platformEvent =
                new PlatformSearchEvent(
                    eventId,
                    eventType,
                    source ?? "Unknown",
                    args.Message.CorrelationId,
                    DateTimeOffset.UtcNow,
                    args.Message.Body.ToString());

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        SearchDbContext>();

            var eventPublisher =
                scope.ServiceProvider
                    .GetRequiredService<
                        IEventPublisher>();

            await HandleAsync(
                dbContext,
                eventPublisher,
                platformEvent,
                args.CancellationToken);

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
                cancellationToken:
                    args.CancellationToken);
        }
    }

    public static async Task<bool> HandleAsync(
        SearchDbContext dbContext,
        IEventPublisher eventPublisher,
        PlatformSearchEvent platformEvent,
        CancellationToken cancellationToken)
    {
        var candidate =
            SearchDocumentFactory.Create(
                platformEvent);

        if (candidate is null)
        {
            return false;
        }

        var existing =
            await dbContext.SearchDocuments
                .Include(x => x.Tags)
                .SingleOrDefaultAsync(
                    x =>
                        x.DocumentType ==
                            candidate.DocumentType &&
                        x.SourceEntityId ==
                            candidate.SourceEntityId,
                    cancellationToken);

        if (existing is null)
        {
            dbContext.SearchDocuments.Add(
                candidate);

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await eventPublisher.PublishAsync(
                new SearchDocumentIndexed(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    candidate.Id,
                    candidate.SourceEntityId,
                    candidate.DocumentType.ToString(),
                    candidate.SourceEventType),
                platformEvent.CorrelationId,
                cancellationToken);

            return true;
        }

        existing.SourceEventType =
            candidate.SourceEventType;

        existing.SourceService =
            candidate.SourceService;

        existing.Title =
            candidate.Title;

        existing.Summary =
            candidate.Summary;

        existing.ExternalReference =
            candidate.ExternalReference;

        existing.CorrelationId =
            candidate.CorrelationId;

        existing.WorkflowId =
            candidate.WorkflowId;

        existing.DiseaseName =
            candidate.DiseaseName;

        existing.GeneSymbol =
            candidate.GeneSymbol;

        existing.CompoundName =
            candidate.CompoundName;

        existing.InchiKey =
            candidate.InchiKey;

        existing.CanonicalSmiles =
            candidate.CanonicalSmiles;

        existing.Outcome =
            candidate.Outcome;

        existing.Score =
            candidate.Score;

        existing.Status =
            candidate.Status;

        existing.SourceUpdatedAtUtc =
            candidate.SourceUpdatedAtUtc;

        existing.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        dbContext.SearchDocumentTags.RemoveRange(
            existing.Tags);

        existing.Tags.Clear();

        foreach (var tag in candidate.Tags)
        {
            existing.Tags.Add(
                new SearchDocumentTag
                {
                    Id =
                        Guid.NewGuid(),

                    SearchDocumentId =
                        existing.Id,

                    Name =
                        tag.Name,

                    Value =
                        tag.Value
                });
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
