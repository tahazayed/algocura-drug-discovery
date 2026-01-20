using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Algocura.Workflow.Api.Data;
using Algocura.Workflow.Api.Domain;
using Algocura.Workflow.Api.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Workflow.Api.Messaging.Consumers;

public sealed class PlatformWorkflowEventsConsumer
    : ServiceBusConsumerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformWorkflowEventsConsumer> _logger;

    public PlatformWorkflowEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<PlatformWorkflowEventsConsumer> logger)
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

            if (!TryResolveStage(
                    eventType,
                    out var stage,
                    out var externalEntityType))
            {
                await args.CompleteMessageAsync(
                    args.Message,
                    args.CancellationToken);

                return;
            }

            using var document =
                JsonDocument.Parse(
                    args.Message.Body.ToString());

            var payload =
                document.RootElement;

            var workflowId =
                ResolveWorkflowId(
                    payload,
                    args.Message.CorrelationId);

            if (workflowId is null)
            {
                await args.DeadLetterMessageAsync(
                    args.Message,
                    "MissingWorkflowId",
                    "The event does not contain workflowId and the correlation ID is not a workflow GUID.",
                    args.CancellationToken);

                return;
            }

            var eventId =
                ReadGuid(
                    payload,
                    "eventId")
                ?? Guid.Parse(
                    args.Message.MessageId);

            var occurredAtUtc =
                ReadDateTimeOffset(
                    payload,
                    "occurredAtUtc")
                ?? DateTimeOffset.UtcNow;

            var externalEntityId =
                ResolveExternalEntityId(
                    payload,
                    stage);

            var source =
                args.Message.ApplicationProperties
                    .TryGetValue(
                        "source",
                        out var sourceValue)
                    ? sourceValue?.ToString() ??
                      "Unknown"
                    : "Unknown";

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        WorkflowDbContext>();

            var stateMachine =
                scope.ServiceProvider
                    .GetRequiredService<
                        IWorkflowStateMachine>();

            await HandleAsync(
                dbContext,
                stateMachine,
                workflowId.Value,
                eventId,
                eventType!,
                source,
                occurredAtUtc,
                stage,
                externalEntityId,
                externalEntityType,
                $"Processed {eventType}.",
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
                "Failed to process workflow message {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken:
                    args.CancellationToken);
        }
    }

    public static async Task HandleAsync(
        WorkflowDbContext dbContext,
        IWorkflowStateMachine stateMachine,
        Guid workflowId,
        Guid eventId,
        string eventType,
        string source,
        DateTimeOffset occurredAtUtc,
        WorkflowStage stage,
        Guid? externalEntityId,
        string externalEntityType,
        string summary,
        CancellationToken cancellationToken)
    {
        var alreadyProcessed =
            await dbContext.WorkflowEventReceipts
                .AnyAsync(
                    x =>
                        x.EventId ==
                        eventId,
                    cancellationToken);

        if (alreadyProcessed)
        {
            return;
        }

        var workflow =
            await dbContext.Workflows
                .Include(
                    x => x.Stages)
                .SingleOrDefaultAsync(
                    x =>
                        x.Id ==
                        workflowId,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                $"Workflow {workflowId} was not found.");

        stateMachine.CompleteStage(
            workflow,
            stage,
            externalEntityId,
            externalEntityType,
            summary,
            occurredAtUtc);

        dbContext.WorkflowEventReceipts.Add(
            new WorkflowEventReceipt
            {
                EventId =
                    eventId,

                WorkflowId =
                    workflow.Id,

                EventType =
                    eventType,

                Source =
                    source,

                OccurredAtUtc =
                    occurredAtUtc,

                ProcessedAtUtc =
                    DateTimeOffset.UtcNow
            });

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static bool TryResolveStage(
        string? eventType,
        out WorkflowStage stage,
        out string externalEntityType)
    {
        (stage, externalEntityType) =
            eventType switch
            {
                "LiteratureIntelligenceSnapshot" =>
                    (
                        WorkflowStage.LiteratureIntelligence,
                        "TargetHypothesis"
                    ),

                "TargetDiscoverySnapshot" =>
                    (
                        WorkflowStage.TargetDiscovery,
                        "TargetAssessment"
                    ),

                "ChemicalDesignSnapshot" =>
                    (
                        WorkflowStage.ChemicalDesign,
                        "CandidateStructure"
                    ),

                "CompoundCatalogSnapshot" =>
                    (
                        WorkflowStage.CompoundCatalog,
                        "Compound"
                    ),

                "CompletedScreeningSnapshot" =>
                    (
                        WorkflowStage.Screening,
                        "Screening"
                    ),

                "AssaySnapshot" =>
                    (
                        WorkflowStage.AssayManagement,
                        "Assay"
                    ),

                "PredictionCompleted" or
                "AiDiscoverySnapshot" =>
                    (
                        WorkflowStage.AiDiscovery,
                        "Prediction"
                    ),

                _ =>
                    (
                        default,
                        string.Empty
                    )
            };

        return !string.IsNullOrWhiteSpace(
            externalEntityType);
    }

    private static Guid? ResolveWorkflowId(
        JsonElement payload,
        string? correlationId)
    {
        return ReadGuid(
                   payload,
                   "workflowId")
            ?? (
                Guid.TryParse(
                    correlationId,
                    out var parsed)
                    ? parsed
                    : null
            );
    }

    private static Guid? ResolveExternalEntityId(
        JsonElement payload,
        WorkflowStage stage)
    {
        var propertyName =
            stage switch
            {
                WorkflowStage.LiteratureIntelligence =>
                    "targetHypothesisId",

                WorkflowStage.TargetDiscovery =>
                    "targetAssessmentId",

                WorkflowStage.ChemicalDesign =>
                    "candidateStructureId",

                WorkflowStage.CompoundCatalog =>
                    "compoundId",

                WorkflowStage.Screening =>
                    "screeningId",

                WorkflowStage.AssayManagement =>
                    "assayId",

                WorkflowStage.AiDiscovery =>
                    "predictionId",

                _ =>
                    null
            };

        return propertyName is null
            ? null
            : ReadGuid(
                payload,
                propertyName);
    }

    private static Guid? ReadGuid(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        return property.ValueKind ==
               JsonValueKind.String &&
               Guid.TryParse(
                   property.GetString(),
                   out var value)
            ? value
            : null;
    }

    private static DateTimeOffset? ReadDateTimeOffset(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            return null;
        }

        return property.ValueKind ==
               JsonValueKind.String &&
               DateTimeOffset.TryParse(
                   property.GetString(),
                   out var value)
            ? value
            : null;
    }
}
