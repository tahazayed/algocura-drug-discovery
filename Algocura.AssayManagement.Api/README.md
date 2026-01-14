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
