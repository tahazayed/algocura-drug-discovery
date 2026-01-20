# Algocura Target Discovery API

The **Algocura Target Discovery API** is the second service in the
Algocura discovery workflow:

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

The service consumes literature-derived target hypotheses, maintains
local projections, manages target assessments, calculates prototype
assessment scores, selects targets, and publishes selected-target
snapshots for the Chemical Design API.

## Prototype limitation

The included scoring engine is deterministic and intended to validate
the workflow and service architecture. It does not scientifically
establish target validity, safety, or druggability. Human scientific
review and validated experimental evidence remain required.

## Features

- Literature target-hypothesis synchronization
- Disease and gene target projections
- Genetic evidence scoring
- Biological plausibility scoring
- Druggability scoring
- Safety-risk scoring
- Translational-potential scoring
- Deterministic overall scoring
- Target selection
- Selected-target synchronization
- SQL Server and Entity Framework Core
- Azure Service Bus consumer and publisher
- Swagger, ReDoc, and health endpoints
- Docker support
- xUnit, FluentAssertions, NSubstitute, and SQLite tests

## Local endpoints

| Resource | URL |
|---|---|
| Service information | `http://localhost:7002/` |
| Swagger | `http://localhost:7002/swagger` |
| ReDoc | `http://localhost:7002/redoc` |
| OpenAPI | `http://localhost:7002/openapi/v1.json` |
| Health | `http://localhost:7002/health` |

## API routes

### Target hypotheses

- `GET /api/v1/target-hypotheses`
- `GET /api/v1/target-hypotheses/{id}`

### Target assessments

- `GET /api/v1/target-assessments`
- `GET /api/v1/target-assessments/{id}`
- `POST /api/v1/target-assessments`
- `PUT /api/v1/target-assessments/{id}`
- `POST /api/v1/target-assessments/{id}/complete`
- `POST /api/v1/target-assessments/{id}/select`
- `DELETE /api/v1/target-assessments/{id}`

### Synchronization

- `POST /api/v1/target-discovery/synchronization`
- `POST /api/v1/target-discovery/synchronization/{id}`

## Event consumed

- `LiteratureIntelligenceSnapshot`

## Events published

- `TargetAssessmentCreated`
- `TargetAssessmentCompleted`
- `TargetSelected`
- `TargetDiscoverySnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()` so it can run immediately.

After validating the model, create the initial migration:

```powershell
dotnet ef migrations add InitialCreate `
  --project .\Algocura.TargetDiscovery.Api\Algocura.TargetDiscovery.Api.csproj `
  --startup-project .\Algocura.TargetDiscovery.Api\Algocura.TargetDiscovery.Api.csproj `
  --context TargetDiscoveryDbContext `
  --output-dir Migrations
```

Then replace `EnsureCreatedAsync()` with:

```csharp
await dbContext.Database.MigrateAsync(cancellationToken);
```

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

This service consumes events and its consumer now derives from `ServiceBusConsumerBase`.

Do not add API-local copies of `IEventPublisher`, `ServiceBusEventPublisher`,
or `ServiceBusOptions`.

