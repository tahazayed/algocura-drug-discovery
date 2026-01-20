using Algocura.ChemicalDesign.Api.Data;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.ChemicalDesign.Api.Messaging.Consumers;

public sealed class TargetDiscoveryEventsConsumer
    : ServiceBusConsumerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TargetDiscoveryEventsConsumer> _logger;

    public TargetDiscoveryEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<TargetDiscoveryEventsConsumer> logger)
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

            if (eventType !=
                nameof(TargetDiscoverySnapshot))
            {
                await args.CompleteMessageAsync(
                    args.Message,
                    args.CancellationToken);

                return;
            }

            var integrationEvent =
                args.Message.Body
                    .ToObjectFromJson<
                        TargetDiscoverySnapshot>(
                        JsonOptions);

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        ChemicalDesignDbContext>();

            await HandleAsync(
                dbContext,
                integrationEvent,
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

    public static async Task HandleAsync(
        ChemicalDesignDbContext dbContext,
        TargetDiscoverySnapshot? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "TargetDiscoverySnapshot payload is empty.");
        }

        var entity =
            await dbContext.SelectedTargets
                .SingleOrDefaultAsync(
                    x =>
                        x.TargetAssessmentId ==
                        integrationEvent.TargetAssessmentId,
                    cancellationToken);

        if (entity is null)
        {
            entity =
                new SelectedTargetProjection
                {
                    TargetAssessmentId =
                        integrationEvent.TargetAssessmentId,

                    TargetHypothesisId =
                        integrationEvent.TargetHypothesisId,

                    DiseaseName =
                        integrationEvent.DiseaseName
                            .Trim(),

                    GeneSymbol =
                        integrationEvent.GeneSymbol
                            .Trim()
                            .ToUpperInvariant(),

                    ProteinTarget =
                        integrationEvent.ProteinTarget?
                            .Trim(),

                    DesiredModulation =
                        integrationEvent.DesiredModulation
                            .Trim(),

                    Rationale =
                        integrationEvent.Rationale
                            .Trim(),

                    LiteratureEvidenceScore =
                        integrationEvent
                            .LiteratureEvidenceScore,

                    TargetAssessmentScore =
                        integrationEvent.OverallScore,

                    ConfidenceLevel =
                        integrationEvent.ConfidenceLevel
                            .Trim(),

                    SourceUpdatedAtUtc =
                        integrationEvent.OccurredAtUtc,

                    SynchronizedAtUtc =
                        DateTimeOffset.UtcNow
                };

            dbContext.SelectedTargets.Add(
                entity);
        }
        else
        {
            entity.TargetHypothesisId =
                integrationEvent.TargetHypothesisId;

            entity.DiseaseName =
                integrationEvent.DiseaseName.Trim();

            entity.GeneSymbol =
                integrationEvent.GeneSymbol
                    .Trim()
                    .ToUpperInvariant();

            entity.ProteinTarget =
                integrationEvent.ProteinTarget?
                    .Trim();

            entity.DesiredModulation =
                integrationEvent.DesiredModulation
                    .Trim();

            entity.Rationale =
                integrationEvent.Rationale.Trim();

            entity.LiteratureEvidenceScore =
                integrationEvent
                    .LiteratureEvidenceScore;

            entity.TargetAssessmentScore =
                integrationEvent.OverallScore;

            entity.ConfidenceLevel =
                integrationEvent.ConfidenceLevel
                    .Trim();

            entity.SourceUpdatedAtUtc =
                integrationEvent.OccurredAtUtc;

            entity.SynchronizedAtUtc =
                DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
