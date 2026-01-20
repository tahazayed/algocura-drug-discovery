# Algocura Literature Intelligence API

The service starts the Algocura discovery workflow:

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
AI Discovery API
```

It registers publications, extracts reviewable disease-gene evidence,
supports human validation, creates target hypotheses, and publishes
integration events for downstream services.

## Prototype limitation

`RuleBasedLiteratureAnalysisService` is deterministic placeholder logic.
It is not a biomedical language model and does not scientifically validate
a disease-gene relationship. Human review remains required.

## Local URLs

- `http://localhost:7004/`
- `http://localhost:7004/swagger`
- `http://localhost:7004/redoc`
- `http://localhost:7004/openapi/v1.json`
- `http://localhost:7004/health`

## Routes

- `GET /api/v1/publications`
- `GET /api/v1/publications/{id}`
- `POST /api/v1/publications`
- `POST /api/v1/publications/{id}/analyze`
- `DELETE /api/v1/publications/{id}`
- `GET /api/v1/evidence`
- `POST /api/v1/evidence/{id}/validate`
- `GET /api/v1/target-hypotheses`
- `GET /api/v1/target-hypotheses/{id}`
- `POST /api/v1/target-hypotheses`
- `POST /api/v1/target-hypotheses/{id}/publish`
- `POST /api/v1/literature-intelligence/synchronization`

## Events

- `PublicationRegistered`
- `LiteratureEvidenceDiscovered`
- `TargetHypothesisProposed`
- `LiteratureIntelligenceSnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()` so it runs immediately.
After validating the model:

```powershell
dotnet ef migrations add InitialCreate `
  --project .\Algocura.LiteratureIntelligence.Api\Algocura.LiteratureIntelligence.Api.csproj `
  --startup-project .\Algocura.LiteratureIntelligence.Api\Algocura.LiteratureIntelligence.Api.csproj `
  --context LiteratureIntelligenceDbContext `
  --output-dir Migrations
```

Then replace `EnsureCreatedAsync()` with `MigrateAsync()`.

## Build and test

```bash
dotnet restore
dotnet build
dotnet test
```

---

## Shared Platform Messaging Refactor

This service uses `Algocura.Platform.Messaging` for shared Azure Service Bus
infrastructure.

The platform library owns:

```text
ServiceBusOptions
ServiceBusClient
IPlatformMessagePublisher
ServiceBusMessagePublisher
IEventPublisher
ServiceBusEventPublisher
ServiceBusAuditEventPublisher
ServiceBusConsumerBase
```

The service keeps its own domain-specific integration-event contracts and
domain-specific message handling.

This service is publisher-only and does not require a Service Bus subscription.

Do not add API-local copies of `IEventPublisher`, `ServiceBusEventPublisher`,
or `ServiceBusOptions`.

