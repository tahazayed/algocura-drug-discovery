# Algocura Assay Management API

The service extends the Algocura discovery workflow:

```text
Literature Intelligence API
        ↓
Target Discovery API
        ↓
Chemical Design API
        ↓
Compound Catalog API
        ↓
Screening API
        ↓
Assay Management API
        ↓
AI Discovery API
```

It consumes completed-screening snapshots, manages assays and measurements,
calculates compound-level assay results, applies quality-control state,
publishes assay snapshots, and supports downstream synchronization.

## Prototype limitation

`DeterministicAssayResultCalculator` calculates means, sample standard
deviation, confidence, and simple outcomes. It does not calculate validated
dose-response curves, IC50, EC50, Ki, or Kd values. Those fields are included
in the model for future validated analytical implementations.

## Local URLs

- `http://localhost:7007/`
- `http://localhost:7007/swagger`
- `http://localhost:7007/redoc`
- `http://localhost:7007/openapi/v1.json`
- `http://localhost:7007/health`

## Main routes

- `GET /api/v1/completed-screenings`
- `GET /api/v1/completed-screenings/{id}`
- `GET /api/v1/assays`
- `GET /api/v1/assays/{id}`
- `POST /api/v1/assays`
- `POST /api/v1/assays/{id}/measurements`
- `POST /api/v1/assays/{id}/complete`
- `POST /api/v1/assays/{id}/publish`
- `POST /api/v1/assay-management/synchronization`
- `POST /api/v1/assay-management/synchronization/{id}`

## Events

Consumed:

- `CompletedScreeningSnapshot`

Published:

- `AssayCreated`
- `AssayCompleted`
- `AssaySnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()`.

```powershell
Add-Migration InitialCreate `
  -Context AssayManagementDbContext `
  -OutputDir Data/Migrations

Update-Database `
  -Context AssayManagementDbContext
```

Then replace `EnsureCreatedAsync()` with:

```csharp
await dbContext.Database.MigrateAsync(cancellationToken);
```

---

## Shared Platform Messaging

`Algocura.AssayManagement.Api` uses `Algocura.Platform.Messaging` for common
Azure Service Bus transport and consumer infrastructure.

The API no longer owns local implementations of:

```text
IEventPublisher
ServiceBusEventPublisher
ServiceBusClient registration
ServiceBusOptions
Service Bus processor lifecycle
```

Published Assay Management events flow through:

```text
AssayCreated / AssayCompleted / AssaySnapshot
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

Audit events use the same underlying transport:

```text
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
Azure Service Bus
```

### Shared event contract

Assay Management events now inherit from the platform
`IntegrationEvent` contract through:

```text
AssayManagementIntegrationEvent
        ↓
Algocura.Platform.Messaging.Events.IntegrationEvent
```

`AssayManagementIntegrationEvent` preserves the service source:

```text
Algocura.AssayManagement.Api
```

Domain-specific events remain owned by this API:

```text
AssayCreated
AssayCompleted
AssaySnapshot
```

The consumed `CompletedScreeningSnapshot` remains a local incoming contract
owned by the Assay Management integration boundary.

### Shared consumer base

`ScreeningEventsConsumer` now inherits from:

```text
ServiceBusConsumerBase
```

The shared base owns:

```text
ServiceBusProcessor creation
Topic/subscription configuration
Processor startup
Processor shutdown
Processor disposal
Cancellation handling
Common processor error logging
Event-type resolution
Source resolution
Event-ID resolution
```

`ScreeningEventsConsumer` continues to own the Assay Management-specific
processing of `CompletedScreeningSnapshot`.

### Registration

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

builder.Services.AddHostedService<
    ScreeningEventsConsumer>();
```

`AddAlgocuraPlatformMessaging(...)` already registers:

```text
ServiceBusClient
IPlatformMessagePublisher -> ServiceBusMessagePublisher
IEventPublisher           -> ServiceBusEventPublisher
ServiceBusAuditEventPublisher
```

Do not register another API-local `IEventPublisher`,
`ServiceBusEventPublisher`, or `ServiceBusClient`.

### Service Bus configuration

```json
"ServiceBus": {
  "UseEmulator": true,
  "ConnectionString": "...",
  "FullyQualifiedNamespace": "",
  "TopicName": "algocura-events",
  "SubscriptionName": "assay-management-api",
  "DefaultMessageTimeToLiveDays": 7,
  "MaxConcurrentCalls": 4,
  "PrefetchCount": 0,
  "MaxAutoLockRenewalMinutes": 5
}
```

`SubscriptionName` is required because Assay Management consumes completed
screening events.

### Auditing configuration

```json
"Auditing": {
  "ServiceName": "Algocura.AssayManagement.Api",
  "PublishEntityAuditEvents": true,
  "StampCreatedAndUpdatedFields": true,
  "IncludeUnchangedPropertiesOnCreate": true,
  "IncludeUnchangedPropertiesOnDelete": true,
  "IgnoreAuditInfrastructureEntities": true
}
```

