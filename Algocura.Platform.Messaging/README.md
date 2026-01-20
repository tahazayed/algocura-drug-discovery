# Algocura.Platform.Messaging

Shared messaging infrastructure for the Algocura platform.

`Algocura.Platform.Messaging` centralizes the common Azure Service Bus
plumbing used by Algocura microservices. It provides shared
integration-event contracts, message publishing abstractions, Azure
Service Bus publisher implementations, audit-event publishing, common
messaging configuration, dependency-injection registration, and a
reusable base class for Service Bus consumers.

The library is intentionally transport-focused. Domain-specific events
and domain processing remain owned by the individual microservices.

------------------------------------------------------------------------

## Responsibilities

The project provides:

-   Shared integration-event contracts.
-   A common event publisher abstraction.
-   A lower-level platform message publisher abstraction.
-   Azure Service Bus message publishing.
-   Standard Service Bus application properties.
-   Audit-event publishing for `Algocura.Platform.Auditing`.
-   Centralized Service Bus configuration.
-   Azure Service Bus client registration.
-   Azure Service Bus emulator support.
-   Azure authentication through `DefaultAzureCredential`.
-   Reusable Service Bus consumer lifecycle management.
-   Common helpers for resolving event type, source, and event ID from
    received messages.

The project does **not** contain business-domain events such as
`CompoundUpdated`, `PredictionCompleted`, `AssaySnapshot`, or
`WorkflowCompleted`. Those contracts remain in their owning services.

------------------------------------------------------------------------

## Project Structure

``` text
Algocura.Platform.Messaging
│
├── Consumers
│   └── ServiceBusConsumerBase.cs
│
├── DependencyInjection
│   └── MessagingServiceCollectionExtensions.cs
│
├── Events
│   ├── IIntegrationEvent.cs
│   └── IntegrationEvent.cs
│
├── Options
│   └── ServiceBusOptions.cs
│
└── Publishing
    ├── IEventPublisher.cs
    ├── IPlatformMessagePublisher.cs
    ├── ServiceBusAuditEventPublisher.cs
    ├── ServiceBusEventPublisher.cs
    └── ServiceBusMessagePublisher.cs
```

------------------------------------------------------------------------

## Architecture

``` text
                    Algocura Microservice
                           │
                           │ Domain event
                           v
                  IEventPublisher
                           │
                           v
                ServiceBusEventPublisher
                           │
                           v
              IPlatformMessagePublisher
                           │
                           v
              ServiceBusMessagePublisher
                           │
                           v
                  Azure Service Bus
                    algocura-events
                           │
             ┌─────────────┼─────────────┐
             │             │             │
             v             v             v
        Subscription  Subscription  Subscription
             │             │             │
             v             v             v
       Microservice   Microservice   Microservice
         Consumer       Consumer       Consumer
```

Audit events use the same underlying transport:

``` text
Entity Framework Core
        │
        v
EntityAuditEventInterceptor
        │
        v
ServiceBusAuditEventPublisher
        │
        v
IPlatformMessagePublisher
        │
        v
Azure Service Bus
```

This allows business integration events and entity audit events to share
the Service Bus infrastructure without coupling the auditing library
directly to Azure Service Bus.

------------------------------------------------------------------------

## Event Contract

### IIntegrationEvent

`IIntegrationEvent` defines the metadata required for a standard
Algocura integration event.

``` csharp
public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }

    string EventType { get; }

    string EventVersion { get; }

    string Source { get; }
}
```

### IntegrationEvent

Services can inherit from the shared `IntegrationEvent` record instead
of defining their own base event type.

``` csharp
public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : IIntegrationEvent
{
    public abstract string EventType { get; }

    public virtual string EventVersion =>
        "1.0";

    public abstract string Source { get; }
}
```

The default event version is `1.0`. A service can override it when a
contract version changes.

------------------------------------------------------------------------

## Creating a Domain Event

Domain events remain in the service that owns them.

Example from a compound-oriented service:

``` csharp
using Algocura.Platform.Messaging.Events;

namespace Algocura.CompoundCatalog.Api.Events;

public sealed record CompoundUpdated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid CompoundId,
    string Name,
    string CanonicalSmiles)
    : IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string EventType =>
        nameof(CompoundUpdated);

    public override string Source =>
        "Algocura.CompoundCatalog.Api";
}
```

The shared messaging project defines the transport contract; it does not
need to know the fields specific to `CompoundUpdated`.

------------------------------------------------------------------------

## Publishing Events

### IEventPublisher

Application code should normally depend on `IEventPublisher`.

``` csharp
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
```

Example:

``` csharp
public sealed class CompoundService(
    IEventPublisher eventPublisher)
{
    public async Task PublishUpdatedAsync(
        CompoundUpdated integrationEvent,
        string? correlationId,
        CancellationToken cancellationToken)
    {
        await eventPublisher.PublishAsync(
            integrationEvent,
            correlationId,
            cancellationToken);
    }
}
```

`ServiceBusEventPublisher` converts the integration-event metadata into
standard Service Bus application properties and delegates the transport
operation to `IPlatformMessagePublisher`.

------------------------------------------------------------------------

## Platform Message Publisher

`IPlatformMessagePublisher` is the lower-level abstraction used when a
component needs to publish a message without depending on
`IIntegrationEvent`.

``` csharp
public interface IPlatformMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message,
        string eventType,
        Guid messageId,
        string? correlationId = null,
        IReadOnlyDictionary<string, object>? applicationProperties = null,
        CancellationToken cancellationToken = default);
}
```

Most application services should use `IEventPublisher`.

`IPlatformMessagePublisher` is primarily intended for infrastructure
integrations such as auditing or other platform-level publishers.

------------------------------------------------------------------------

## Standard Message Metadata

`ServiceBusEventPublisher` adds the following application properties to
integration events:

  Property          Description
  ----------------- ---------------------------------------
  `eventType`       Integration-event type
  `eventVersion`    Contract version
  `source`          Publishing service
  `occurredAtUtc`   UTC timestamp when the event occurred

The Service Bus message also contains:

  Service Bus Field   Value
  ------------------- -------------------------------------------
  `MessageId`         `EventId`
  `Subject`           `EventType`
  `CorrelationId`     Supplied correlation ID
  `ContentType`       `application/json;charset=utf-8`
  `TimeToLive`        Configured `DefaultMessageTimeToLiveDays`

This creates a consistent message envelope across Algocura services.

------------------------------------------------------------------------

## Auditing Integration

`ServiceBusAuditEventPublisher` implements the audit publisher contract
from `Algocura.Platform.Auditing`.

It allows EF Core entity changes captured by the auditing infrastructure
to be published through the same platform Service Bus publisher.

Register auditing with:

``` csharp
builder.Services
    .AddAlgocuraAuditing<
        ServiceBusAuditEventPublisher>(
        options =>
        {
            builder.Configuration
                .GetSection(
                    AuditingOptions.SectionName)
                .Bind(options);
        });
```

The audit publisher adds audit-specific message properties including:

``` text
eventVersion
serviceName
entityType
action
occurredAtUtc
```

The service name belongs to the audit event because it identifies the
microservice where the audited entity change occurred. Service Bus
itself does not replace this information; it only transports the event.

------------------------------------------------------------------------

## Configuration

The library binds configuration from the `ServiceBus` section.

Example for a service that only publishes:

``` json
{
  "ServiceBus": {
    "UseEmulator": true,
    "ConnectionString": "<service-bus-connection-string>",
    "FullyQualifiedNamespace": "",
    "TopicName": "algocura-events",
    "DefaultMessageTimeToLiveDays": 7,
    "MaxConcurrentCalls": 4,
    "PrefetchCount": 0,
    "MaxAutoLockRenewalMinutes": 5
  }
}
```

Example for a service that consumes events:

``` json
{
  "ServiceBus": {
    "UseEmulator": true,
    "ConnectionString": "<service-bus-connection-string>",
    "FullyQualifiedNamespace": "",
    "TopicName": "algocura-events",
    "SubscriptionName": "screening-api",
    "DefaultMessageTimeToLiveDays": 7,
    "MaxConcurrentCalls": 4,
    "PrefetchCount": 0,
    "MaxAutoLockRenewalMinutes": 5
  }
}
```

### ServiceBusOptions

  ------------------------------------------------------------------------------------------
  Property                         Purpose                                           Default
  -------------------------------- ---------------------------- ----------------------------
  `UseEmulator`                    Select emulator or Azure                          `false`
                                   Service Bus                  

  `ConnectionString`               Emulator/connection-string                          Empty
                                   configuration                

  `FullyQualifiedNamespace`        Azure Service Bus namespace                         Empty

  `TopicName`                      Shared platform topic                   `algocura-events`

  `SubscriptionName`               Service-specific                                   `null`
                                   subscription for consumers   

  `DefaultMessageTimeToLiveDays`   Published-message TTL                                 `7`

  `MaxConcurrentCalls`             Concurrent consumer                                   `4`
                                   callbacks                    

  `PrefetchCount`                  Service Bus processor                                 `0`
                                   prefetch count               

  `MaxAutoLockRenewalMinutes`      Maximum automatic                                     `5`
                                   message-lock renewal         
  ------------------------------------------------------------------------------------------

`SubscriptionName` is intentionally optional because publisher-only
services do not require a subscription.

A consumer using `ServiceBusConsumerBase` requires `SubscriptionName`.

------------------------------------------------------------------------

## Dependency Injection

Add the platform messaging library once in `Program.cs`:

``` csharp
using Algocura.Platform.Messaging.DependencyInjection;

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

This registers the shared messaging infrastructure:

``` text
ServiceBusOptions
ServiceBusClient
IPlatformMessagePublisher -> ServiceBusMessagePublisher
IEventPublisher           -> ServiceBusEventPublisher
ServiceBusAuditEventPublisher
```

Individual APIs should not register their own `ServiceBusClient`,
`ServiceBusOptions`, `IPlatformMessagePublisher`, or
`ServiceBusEventPublisher` when using this extension.

------------------------------------------------------------------------

## Azure Service Bus Authentication

The registration supports two modes.

### Local emulator

Set:

``` json
"UseEmulator": true
```

and provide:

``` json
"ConnectionString": "..."
```

The library creates:

``` csharp
new ServiceBusClient(
    options.ConnectionString);
```

### Azure

Set:

``` json
"UseEmulator": false
```

and provide the namespace:

``` json
"FullyQualifiedNamespace":
    "my-namespace.servicebus.windows.net"
```

The library creates the client using:

``` csharp
new ServiceBusClient(
    options.FullyQualifiedNamespace,
    new DefaultAzureCredential());
```

The workload therefore uses the Azure Identity credential chain rather
than embedding Service Bus credentials in application code.

------------------------------------------------------------------------

## Consuming Events

Services that consume Service Bus events can inherit from
`ServiceBusConsumerBase`.

The base class owns:

-   `ServiceBusProcessor` creation.
-   Topic and subscription configuration.
-   Processor startup.
-   Processor shutdown.
-   Processor disposal.
-   Cancellation handling.
-   Standard Service Bus error logging.
-   Event-type resolution.
-   Source resolution.
-   Event-ID resolution.

The service remains responsible for:

-   Deciding which domain events it supports.
-   Deserializing domain payloads.
-   Executing domain/application logic.
-   Completing, abandoning, or dead-lettering individual messages.

Example:

``` csharp
using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

public sealed class CompoundEventsConsumer
    : ServiceBusConsumerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CompoundEventsConsumer> _logger;

    public CompoundEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<CompoundEventsConsumer> logger)
        : base(
            client,
            options,
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

            switch (eventType)
            {
                case "CompoundRegistered":
                    await HandleCompoundRegisteredAsync(
                        args);
                    break;

                case "CompoundUpdated":
                    await HandleCompoundUpdatedAsync(
                        args);
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
                cancellationToken:
                    args.CancellationToken);
        }
    }

    private Task HandleCompoundRegisteredAsync(
        ProcessMessageEventArgs args)
    {
        // Service-specific domain processing.
        return Task.CompletedTask;
    }

    private Task HandleCompoundUpdatedAsync(
        ProcessMessageEventArgs args)
    {
        // Service-specific domain processing.
        return Task.CompletedTask;
    }
}
```

Register the consumer in the API:

``` csharp
builder.Services.AddHostedService<
    CompoundEventsConsumer>();
```

------------------------------------------------------------------------

## Consumer Helper Methods

`ServiceBusConsumerBase` exposes common message metadata helpers.

### ResolveEventType

``` csharp
var eventType =
    ResolveEventType(
        args.Message);
```

The helper first checks the `eventType` application property and falls
back to the Service Bus `Subject`.

### ResolveSource

``` csharp
var source =
    ResolveSource(
        args.Message);
```

The helper reads the `source` application property and returns `Unknown`
when no source is present.

### ResolveEventId

``` csharp
var eventId =
    ResolveEventId(
        args.Message);
```

The helper attempts to parse `MessageId` as a GUID.

------------------------------------------------------------------------

## Publisher-Only Service

A publisher-only API requires only the shared registration:

``` csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

Application code injects:

``` csharp
IEventPublisher
```

No `SubscriptionName` is required.

Example:

``` json
{
  "ServiceBus": {
    "UseEmulator": true,
    "ConnectionString": "...",
    "TopicName": "algocura-events"
  }
}
```

------------------------------------------------------------------------

## Publisher and Consumer Service

A service that both publishes and consumes events uses the same platform
registration plus its hosted consumer:

``` csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

builder.Services.AddHostedService<
    PlatformWorkflowEventsConsumer>();
```

Its configuration includes a subscription:

``` json
{
  "ServiceBus": {
    "UseEmulator": true,
    "ConnectionString": "...",
    "TopicName": "algocura-events",
    "SubscriptionName": "workflow-api"
  }
}
```

Each consuming microservice should have its own Service Bus subscription
so that services can process events independently.

------------------------------------------------------------------------

## Recommended API Program.cs Pattern

``` csharp
// -----------------------------------------------------------------------------
// Auditing
// -----------------------------------------------------------------------------

var auditingSection =
    builder.Configuration.GetSection(
        AuditingOptions.SectionName);

builder.Services
    .AddAlgocuraAuditing<
        ServiceBusAuditEventPublisher>(
        options =>
        {
            auditingSection.Bind(
                options);
        });

// -----------------------------------------------------------------------------
// Azure Service Bus
// -----------------------------------------------------------------------------

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

// Only for APIs that consume events.
builder.Services.AddHostedService<
    PlatformEventsConsumer>();
```

Do not add another API-local `ServiceBusOptions` class. Use:

``` csharp
using Algocura.Platform.Messaging.Options;
```

This avoids ambiguous references and keeps Service Bus configuration
consistent across the platform.

------------------------------------------------------------------------

## Service Information Endpoint

An API can expose its effective messaging configuration without exposing
secrets:

``` csharp
app.MapGet(
    "/",
    (
        IOptions<ServiceBusOptions> serviceBusOptions,
        IWebHostEnvironment environment) =>
    {
        var messagingOptions =
            serviceBusOptions.Value;

        return Results.Ok(
            new
            {
                service =
                    "Algocura Example API",

                environment =
                    environment.EnvironmentName,

                messaging =
                    new
                    {
                        provider =
                            "Azure Service Bus",

                        mode =
                            messagingOptions.UseEmulator
                                ? "Emulator"
                                : "Azure",

                        topic =
                            messagingOptions.TopicName,

                        subscription =
                            messagingOptions.SubscriptionName
                    }
            });
    });
```

Do not return the connection string, credentials, keys, or tokens.

------------------------------------------------------------------------

## Migration from API-Local Messaging

For each Algocura API:

1.  Add a project reference to `Algocura.Platform.Messaging`.
2.  Remove the API-local `ServiceBusOptions`.
3.  Remove the API-local `IEventPublisher`.
4.  Remove the API-local `ServiceBusEventPublisher`.
5.  Replace the API-local `IntegrationEvent` base with
    `Algocura.Platform.Messaging.Events.IntegrationEvent`, where
    practical.
6.  Replace manual `ServiceBusClient` registration with
    `AddAlgocuraPlatformMessaging(...)`.
7.  Inject the shared `IEventPublisher` from
    `Algocura.Platform.Messaging.Publishing`.
8.  For consumers, optionally inherit from `ServiceBusConsumerBase`.
9.  Keep domain-specific integration-event types inside the owning API.
10. Keep domain-specific consumer processing inside the consuming API.

After migration, API projects should contain business event definitions
and business event handlers, while the shared project owns the transport
infrastructure.

------------------------------------------------------------------------

## Dependency Direction

The intended dependency direction is:

``` text
Algocura.Platform.Auditing
          ^
          |
Algocura.Platform.Messaging
          ^
          |
     Algocura APIs
```

`Algocura.Platform.Messaging` may implement the audit publishing
abstraction because it provides the Azure Service Bus transport.

The auditing project should not depend on `Algocura.Platform.Messaging`.
This prevents the audit domain abstraction from becoming coupled to a
specific messaging transport.

------------------------------------------------------------------------

## Design Principles

### Centralize transport concerns

Azure Service Bus client creation, message serialization, message
metadata, publisher behavior, and consumer lifecycle should not be
reimplemented by every microservice.

### Keep domain contracts with their owners

`Algocura.Platform.Messaging` should not become a repository for every
event in the system. Domain-specific event contracts remain in their
owning services.

### Depend on abstractions

Business code publishes through `IEventPublisher` rather than directly
creating `ServiceBusMessage` or `ServiceBusSender`.

### Keep auditing transport-independent

`Algocura.Platform.Auditing` defines the audit publishing abstraction.
`Algocura.Platform.Messaging` supplies the Service Bus implementation.

### One ServiceBusClient per application

The Service Bus client is registered as a singleton and reused by the
application.

### Configuration over hard-coded infrastructure

Topic, subscription, emulator mode, namespace, TTL, concurrency,
prefetch, and lock-renewal settings are configuration-driven.

------------------------------------------------------------------------

## Current Shared Components

  ----------------------------------------------------------------------------
  Component                                Responsibility
  ---------------------------------------- -----------------------------------
  `IIntegrationEvent`                      Standard integration-event metadata
                                           contract

  `IntegrationEvent`                       Optional base record for service
                                           events

  `ServiceBusOptions`                      Shared Service Bus configuration

  `IEventPublisher`                        Application-facing
                                           integration-event publisher

  `IPlatformMessagePublisher`              Transport-level publisher
                                           abstraction

  `ServiceBusEventPublisher`               Publishes standard integration
                                           events

  `ServiceBusMessagePublisher`             Creates and sends Service Bus
                                           messages

  `ServiceBusAuditEventPublisher`          Publishes entity audit events

  `ServiceBusConsumerBase`                 Shared Service Bus processor
                                           lifecycle

  `MessagingServiceCollectionExtensions`   Registers messaging infrastructure
  ----------------------------------------------------------------------------

------------------------------------------------------------------------

## Security

-   Do not commit Service Bus connection strings to source control.
-   Prefer Azure identity in deployed environments.
-   Do not place secrets in Service Bus application properties.
-   Do not expose connection strings through health or
    service-information endpoints.
-   Audit-event property filtering and masking should be configured
    through `Algocura.Platform.Auditing`.
-   Treat message payloads as application data and apply the same
    data-classification rules used by the owning service.

------------------------------------------------------------------------

## Summary

`Algocura.Platform.Messaging` provides one consistent messaging layer
for Algocura microservices:

``` text
Domain Event
    ↓
IEventPublisher
    ↓
ServiceBusEventPublisher
    ↓
IPlatformMessagePublisher
    ↓
ServiceBusMessagePublisher
    ↓
Azure Service Bus
    ↓
ServiceBusConsumerBase
    ↓
Service-specific Consumer
    ↓
Domain/Application Logic
```

This keeps Azure Service Bus infrastructure centralized while preserving
microservice ownership of domain events and domain processing.
