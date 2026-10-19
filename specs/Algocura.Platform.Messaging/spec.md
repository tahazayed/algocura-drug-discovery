# Algocura.Platform.Messaging — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 12.

## Original documented intent

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
                ex

## Project definition (authoritative build metadata)

Source: `Algocura.Platform.Messaging/Algocura.Platform.Messaging.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Azure.Identity" Version="1.*" />
		<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.*" />
	</ItemGroup>

	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>

	<ItemGroup>
		<ProjectReference Include="..\Algocura.Platform.Auditing\Algocura.Platform.Auditing.csproj" />
	</ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Platform.Messaging/Algocura.Platform.Messaging.csproj` | 20 |
| `Algocura.Platform.Messaging/Consumers/ServiceBusConsumerBase.cs` | 163 |
| `Algocura.Platform.Messaging/DependencyInjection/MessagingServiceCollectionExtensions.cs` | 90 |
| `Algocura.Platform.Messaging/Events/IIntegrationEvent.cs` | 14 |
| `Algocura.Platform.Messaging/Events/IntegrationEvent.cs` | 14 |
| `Algocura.Platform.Messaging/Options/ServiceBusOptions.cs` | 34 |
| `Algocura.Platform.Messaging/Publishing/IEventPublisher.cs` | 12 |
| `Algocura.Platform.Messaging/Publishing/IPlatformMessagePublisher.cs` | 12 |
| `Algocura.Platform.Messaging/Publishing/ServiceBusAuditEventPublisher.cs` | 55 |
| `Algocura.Platform.Messaging/Publishing/ServiceBusEventPublisher.cs` | 47 |
| `Algocura.Platform.Messaging/Publishing/ServiceBusMessagePublisher.cs` | 107 |
| `Algocura.Platform.Messaging/README.md` | 982 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
