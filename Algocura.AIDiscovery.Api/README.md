# Algocura AI Discovery API

The **Algocura AI Discovery API** manages prototype prediction workflows,
candidate scoring, ranking, prioritization, and integration events.

## Features

- Compound projection synchronization
- Completed-screening projection synchronization
- Prediction jobs
- Deterministic prototype scoring
- Candidate prioritization
- SQL Server with Entity Framework Core
- Azure Service Bus publishing and consuming
- Swagger and ReDoc
- Health checks
- Docker
- xUnit, FluentAssertions, NSubstitute, and SQLite tests

## Endpoints

| Resource | URL |
|---|---|
| Service information | `http://localhost:7003/` |
| Swagger | `http://localhost:7003/swagger` |
| ReDoc | `http://localhost:7003/redoc` |
| OpenAPI | `http://localhost:7003/openapi/v1.json` |
| Health | `http://localhost:7003/health` |

## API routes

- `GET /api/v1/models`
- `GET /api/v1/models/{id}`
- `GET /api/v1/predictions`
- `GET /api/v1/predictions/{id}`
- `POST /api/v1/predictions`
- `POST /api/v1/predictions/{id}/start`
- `POST /api/v1/predictions/{id}/run`
- `POST /api/v1/predictions/{id}/fail`
- `DELETE /api/v1/predictions/{id}`

## Events consumed

- `CompoundRegistered`
- `CompoundUpdated`
- `CompoundDeleted`
- `ScreeningCompleted`

## Events published

- `PredictionCreated`
- `PredictionStarted`
- `PredictionCompleted`
- `PredictionFailed`
- `CandidatePrioritized`

## Build and test

```bash
dotnet restore
dotnet build
dotnet test
```
