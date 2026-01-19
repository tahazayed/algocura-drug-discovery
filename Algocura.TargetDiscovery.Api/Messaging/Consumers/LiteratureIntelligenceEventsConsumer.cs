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
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<LiteratureIntelligenceEventsConsumer> _logger;

    public LiteratureIntelligenceEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> serviceBusOptions,
        IConfiguration configuration,
        ILogger<LiteratureIntelligenceEventsConsumer> logger)
    {
        _scopeFactory =
            scopeFactory;

        _logger =
            logger;

        var messagingOptions =
            serviceBusOptions.Value;

        var subscriptionName =
            configuration[
                "ServiceBus:SubscriptionName"];

        if (string.IsNullOrWhiteSpace(
                subscriptionName))
        {
            throw new InvalidOperationException(
                "ServiceBus:SubscriptionName is required.");
        }

        _processor =
            serviceBusClient.CreateProcessor(
                messagingOptions.TopicName,
                subscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages =
                        false,

                    MaxConcurrentCalls =
                        4,

                    MaxAutoLockRenewalDuration =
                        TimeSpan.FromMinutes(5)
                });
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync +=
            ProcessMessageAsync;

        _processor.ProcessErrorAsync +=
            ProcessErrorAsync;

        await _processor.StartProcessingAsync(
            stoppingToken);

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
            var eventType =
                args.Message.ApplicationProperties
                    .TryGetValue(
                        "eventType",
                        out var value)
                    ? value?.ToString()
                    : args.Message.Subject;

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

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus consumer error. " +
            "Entity: {Entity}; Source: {Source}.",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(
            cancellationToken);

        await base.StopAsync(
            cancellationToken);
    }

    public override void Dispose()
    {
        _processor.DisposeAsync()
            .AsTask()
            .GetAwaiter()
            .GetResult();

        base.Dispose();
    }
}