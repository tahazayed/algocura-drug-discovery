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


---

## Adaptive Molecular-Model Routing

The API now supports a prototype adaptive routing layer using the molecular and
workflow information currently available in AI Discovery: molecular weight,
screening context, prior model confidence, model specialization, and a resource
budget.

```text
POST /api/v1/predictions/{id}/adaptive-run
```

Example:

```json
{ "resourceBudget": 1.0 }
```

The router scores eligible active models using molecular suitability, expected
confidence, specialization, and estimated resource cost. If prototype
confidence is below `AdaptiveRouting:MinimumAcceptedConfidence`, the
orchestrator can route to another eligible model while budget remains.

The current confidence calculation is explicitly a deterministic prototype.
Replace it with model-native calibrated confidence/uncertainty when real ML
models are integrated. The existing `/run` endpoint remains unchanged.

### Seeded adaptive-routing models

The prototype seed now creates a small heterogeneous model catalog so adaptive
routing can demonstrate actual model selection and escalation:

| Model | Role | Expected confidence | Resource cost |
|---|---|---:|---:|
| `QSAR Fast` | Fast first-pass model | 0.64 | 0.10 |
| `Algocura Deterministic Prioritizer` | Baseline deterministic model | 0.70 | 0.16 |
| `GNN Molecular` | Graph-oriented specialist | 0.78 | 0.25 |
| `SMILES Transformer` | Sequence-oriented escalation model | 0.84 | 0.32 |
| `Ensemble Consensus` | High-confidence final escalation | 0.91 | 0.45 |

The model IDs are deterministic so the `AdaptiveRouting:Models` configuration
can reference them safely across repeated development database initialization.

These are prototype model definitions. They currently execute through the
existing deterministic scoring engine with different model parameters; their
names describe the intended future model specialization. When actual QSAR,
GNN, transformer, and ensemble runtimes are integrated, the routing contracts
can remain while the execution implementation is replaced.

