using Algocura.ChemicalDesign.Api.Data;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.ChemicalDesign.Api.Messaging.Consumers;

public sealed class TargetDiscoveryEventsConsumer
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<TargetDiscoveryEventsConsumer> _logger;

    public TargetDiscoveryEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        IConfiguration configuration,
        ILogger<TargetDiscoveryEventsConsumer> logger)
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
            client.CreateProcessor(
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

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus error. Entity: {Entity}; Source: {Source}.",
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