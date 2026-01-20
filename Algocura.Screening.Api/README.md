# Algocura Screening API

A .NET 10 microservice that creates and tracks virtual-screening jobs, maintains a local projection of compounds received from the Compound Catalog API, persists screening state in SQL Server through EF Core, and publishes lifecycle events through Azure Service Bus.

## Projects

- `Algocura.Screening.Api`: ASP.NET Core controller API, EF Core SQL Server persistence, Service Bus publisher and consumer.
- `Algocura.Screening.Api.Tests`: xUnit unit tests using EF Core InMemory, FluentAssertions, and NSubstitute.

## API

Base URL: `/api/v1/screenings`

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/v1/screenings` | List screenings with status and paging filters |
| GET | `/api/v1/screenings/{id}` | Get one screening |
| POST | `/api/v1/screenings` | Create a queued screening |
| PUT | `/api/v1/screenings/{id}` | Edit a draft or queued screening |
| POST | `/api/v1/screenings/{id}/start` | Start a queued screening |
| POST | `/api/v1/screenings/{id}/complete` | Complete and rank results |
| POST | `/api/v1/screenings/{id}/fail` | Mark queued/running screening failed |
| POST | `/api/v1/screenings/{id}/cancel` | Cancel a nonterminal screening |
| DELETE | `/api/v1/screenings/{id}` | Delete a non-running screening |

## Events

Consumes `CompoundRegisteredMessage`, `CompoundUpdatedMessage`, and `CompoundDeletedMessage` from `compound-events` / `screening-api`.

Publishes `ScreeningCreated`, `ScreeningStarted`, `ScreeningCompleted`, `ScreeningFailed`, and `ScreeningCancelled` to `screening-events`.

## Commands

```bash
dotnet restore Algocura.Screening.slnx
dotnet build Algocura.Screening.slnx
dotnet test Algocura.Screening.slnx
```

Create a migration after changing the model:

```bash
dotnet ef migrations add MigrationName --project Algocura.Screening.Api --startup-project Algocura.Screening.Api
```

Run locally:

```bash
dotnet run --project Algocura.Screening.Api
```

Swagger: `http://localhost:7002/swagger`
Health: `http://localhost:7002/health`

## Important integration contract note

The compound event record names and fields must exactly match the events published by `Algocura.CompoundCatalog.Api`. Move shared event contracts into a versioned contracts package when the second service is integrated.

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

