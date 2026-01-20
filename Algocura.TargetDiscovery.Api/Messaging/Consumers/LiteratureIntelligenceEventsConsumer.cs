using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Algocura.TargetDiscovery.Api.Data;
using Algocura.TargetDiscovery.Api.Domain;
using Algocura.TargetDiscovery.Api.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.TargetDiscovery.Api.Messaging.Consumers;

public sealed class LiteratureIntelligenceEventsConsumer
    : ServiceBusConsumerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiteratureIntelligenceEventsConsumer> _logger;

    public LiteratureIntelligenceEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<LiteratureIntelligenceEventsConsumer> logger)
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
            var eventType =
                ResolveEventType(
                    args.Message);

            if (eventType !=
                nameof(LiteratureIntelligenceSnapshot))
            {
                _logger.LogDebug(
                    "Ignoring unsupported event type {EventType}.",
                    eventType);

                await args.CompleteMessageAsync(
                    args.Message,
                    args.CancellationToken);

                return;
            }

            var integrationEvent =
                args.Message.Body
                    .ToObjectFromJson<
                        LiteratureIntelligenceSnapshot>(
                        JsonOptions);

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        TargetDiscoveryDbContext>();

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
        TargetDiscoveryDbContext dbContext,
        LiteratureIntelligenceSnapshot? integrationEvent,
        CancellationToken cancellationToken)
    {
        if (integrationEvent is null)
        {
            throw new JsonException(
                "LiteratureIntelligenceSnapshot payload is empty.");
        }

        var hypothesis =
            await dbContext.TargetHypotheses
                .SingleOrDefaultAsync(
                    x =>
                        x.TargetHypothesisId ==
                        integrationEvent.TargetHypothesisId,
                    cancellationToken);

        if (hypothesis is null)
        {
            hypothesis =
                new TargetHypothesisProjection
                {
                    TargetHypothesisId =
                        integrationEvent.TargetHypothesisId,

                    DiseaseName =
                        integrationEvent.DiseaseName.Trim(),

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
                        integrationEvent.EvidenceScore,

                    Status =
                        TargetHypothesisProjectionStatus.Published,

                    SourceUpdatedAtUtc =
                        integrationEvent.OccurredAtUtc,

                    SynchronizedAtUtc =
                        DateTimeOffset.UtcNow
                };

            dbContext.TargetHypotheses.Add(
                hypothesis);
        }
        else
        {
            hypothesis.DiseaseName =
                integrationEvent.DiseaseName.Trim();

            hypothesis.GeneSymbol =
                integrationEvent.GeneSymbol
                    .Trim()
                    .ToUpperInvariant();

            hypothesis.ProteinTarget =
                integrationEvent.ProteinTarget?
                    .Trim();

            hypothesis.DesiredModulation =
                integrationEvent.DesiredModulation
                    .Trim();

            hypothesis.Rationale =
                integrationEvent.Rationale.Trim();

            hypothesis.LiteratureEvidenceScore =
                integrationEvent.EvidenceScore;

            hypothesis.Status =
                TargetHypothesisProjectionStatus.Published;

            hypothesis.SourceUpdatedAtUtc =
                integrationEvent.OccurredAtUtc;

            hypothesis.SynchronizedAtUtc =
                DateTimeOffset.UtcNow;
        }

        var existingEvidence =
            await dbContext.TargetHypothesisEvidence
                .Where(
                    x =>
                        x.TargetHypothesisId ==
                        integrationEvent.TargetHypothesisId)
                .ToListAsync(
                    cancellationToken);

        if (existingEvidence.Count > 0)
        {
            dbContext.TargetHypothesisEvidence.RemoveRange(
                existingEvidence);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var evidenceLinks =
            integrationEvent.EvidenceStatementIds
                .Distinct()
                .Select(
                    evidenceStatementId =>
                        new TargetHypothesisEvidenceProjection
                        {
                            TargetHypothesisId =
                                integrationEvent.TargetHypothesisId,

                            EvidenceStatementId =
                                evidenceStatementId
                        })
                .ToArray();

        if (evidenceLinks.Length > 0)
        {
            dbContext.TargetHypothesisEvidence.AddRange(
                evidenceLinks);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}
