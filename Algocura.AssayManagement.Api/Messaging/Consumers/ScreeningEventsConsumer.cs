using Algocura.AssayManagement.Api.Data;
using Algocura.AssayManagement.Api.Domain;
using Algocura.AssayManagement.Api.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.AssayManagement.Api.Messaging.Consumers;

public sealed class ScreeningEventsConsumer
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<ScreeningEventsConsumer> _logger;

    public ScreeningEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<ScreeningEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        _processor = client.CreateProcessor(
            options.Value.TopicName,
            options.Value.SubscriptionName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 4,
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
                nameof(CompletedScreeningSnapshot))
            {
                await args.CompleteMessageAsync(
                    args.Message,
                    args.CancellationToken);

                return;
            }

            var integrationEvent =
                args.Message.Body
                    .ToObjectFromJson<CompletedScreeningSnapshot>(
                        JsonOptions);

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AssayManagementDbContext>();

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
        AssayManagementDbContext dbContext,
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
                x =>
                    x.ScreeningId ==
                    integrationEvent.ScreeningId,
                cancellationToken);

        if (screening is null)
        {
            screening = new CompletedScreeningProjection
            {
                ScreeningId =
                    integrationEvent.ScreeningId,
                Name =
                    integrationEvent.Name.Trim(),
                ScreeningMethod =
                    integrationEvent.ScreeningMethod.Trim(),
                CompletedAtUtc =
                    integrationEvent.CompletedAtUtc,
                Status =
                    ScreeningProjectionStatus.Completed,
                SourceUpdatedAtUtc =
                    integrationEvent.OccurredAtUtc,
                SynchronizedAtUtc =
                    DateTimeOffset.UtcNow
            };

            dbContext.Screenings.Add(screening);
        }
        else
        {
            screening.Name =
                integrationEvent.Name.Trim();

            screening.ScreeningMethod =
                integrationEvent.ScreeningMethod.Trim();

            screening.CompletedAtUtc =
                integrationEvent.CompletedAtUtc;

            screening.Status =
                ScreeningProjectionStatus.Completed;

            screening.SourceUpdatedAtUtc =
                integrationEvent.OccurredAtUtc;

            screening.SynchronizedAtUtc =
                DateTimeOffset.UtcNow;
        }

        var existingResults =
            await dbContext.ScreeningResults
                .Where(
                    x =>
                        x.ScreeningId ==
                        integrationEvent.ScreeningId)
                .ToListAsync(cancellationToken);

        if (existingResults.Count > 0)
        {
            dbContext.ScreeningResults.RemoveRange(
                existingResults);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var replacementResults =
            integrationEvent.Results
                .Select(
                    result =>
                        new CompletedScreeningResultProjection
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
