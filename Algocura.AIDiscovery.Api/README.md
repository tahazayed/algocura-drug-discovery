# Algocura AI Discovery API

The AI Discovery API manages compound and screening projections, prediction
jobs, deterministic prototype scoring, candidate prioritization, and
downstream integration events.

## Shared Platform Messaging

The API uses `Algocura.Platform.Messaging` for common Azure Service Bus
infrastructure.

The API no longer owns local implementations of:

```text
IEventPublisher
ServiceBusEventPublisher
ServiceBusClient registration
ServiceBusOptions
Service Bus processor lifecycle
```

Business events publish through:

```text
AI Discovery event
        ↓
Algocura.Platform.Messaging.Publishing.IEventPublisher
        ↓
ServiceBusEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
ServiceBusMessagePublisher
        ↓
Azure Service Bus
```

Audit events publish through:

```text
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
Azure Service Bus
```

The AI Discovery domain events inherit from the shared
`Algocura.Platform.Messaging.Events.IntegrationEvent` through the
service-specific `AIDiscoveryIntegrationEvent` base record. This keeps the
service-owned source value:

```text
Algocura.AIDiscovery.Api
```

The `PlatformEventsConsumer` now inherits from `ServiceBusConsumerBase`.
The shared base owns processor creation, startup, shutdown, disposal, common
Service Bus options, and processor error logging. The AI Discovery consumer
continues to own its domain-specific event routing and projection updates.

## Registration

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

No API-local registration of `IEventPublisher` or `ServiceBusEventPublisher`
is required because the shared extension registers them.

Audit events use:

```csharp
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

## Configuration

```json
"ServiceBus": {
  "UseEmulator": true,
  "ConnectionString": "...",
  "FullyQualifiedNamespace": "",
  "TopicName": "algocura-events",
  "SubscriptionName": "ai-discovery-api",
  "DefaultMessageTimeToLiveDays": 7,
  "MaxConcurrentCalls": 4,
  "PrefetchCount": 0,
  "MaxAutoLockRenewalMinutes": 5
},
"Auditing": {
  "ServiceName": "Algocura.AIDiscovery.Api",
  "PublishEntityAuditEvents": true,
  "StampCreatedAndUpdatedFields": true,
  "IncludeUnchangedPropertiesOnCreate": true,
  "IncludeUnchangedPropertiesOnDelete": true,
  "IgnoreAuditInfrastructureEntities": true
}
```

`SubscriptionName` is required because AI Discovery consumes platform events.
