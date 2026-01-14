# Algocura Workflow API

The Workflow API tracks and coordinates the end-to-end discovery process:

```text
Literature Intelligence
        ↓
Target Discovery
        ↓
Chemical Design
        ↓
Compound Catalog
        ↓
Screening
        ↓
Assay Management
        ↓
AI Discovery
```

The service owns workflow state only. It does not directly access another
service's database and does not perform scientific calculations.

## Correlation requirement

Downstream integration events must include either:

- `workflowId` in the JSON payload, or
- the workflow GUID as the Azure Service Bus `CorrelationId`.

This enables the Workflow API to connect an event to the correct workflow.

## Local URLs

- `http://localhost:7008/`
- `http://localhost:7008/swagger`
- `http://localhost:7008/redoc`
- `http://localhost:7008/openapi/v1.json`
- `http://localhost:7008/health`

## Routes

- `GET /api/v1/workflows`
- `GET /api/v1/workflows/{id}`
- `POST /api/v1/workflows`
- `POST /api/v1/workflows/{id}/start`
- `POST /api/v1/workflows/{id}/stages/{stage}/fail`
- `POST /api/v1/workflows/{id}/cancel`
- `POST /api/v1/workflow/synchronization`
- `POST /api/v1/workflow/synchronization/{id}`

## Consumed events

- `LiteratureIntelligenceSnapshot`
- `TargetDiscoverySnapshot`
- `ChemicalDesignSnapshot`
- `CompoundCatalogSnapshot`
- `CompletedScreeningSnapshot`
- `AssaySnapshot`
- `PredictionCompleted` or `AiDiscoverySnapshot`

## Published events

- `WorkflowCreated`
- `WorkflowStageCompleted`
- `WorkflowCompleted`
- `WorkflowSnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()`.

```powershell
Add-Migration InitialCreate `
  -Context WorkflowDbContext `
  -OutputDir Data/Migrations

Update-Database `
  -Context WorkflowDbContext
```

Then replace `EnsureCreatedAsync()` with:

```csharp
await dbContext.Database.MigrateAsync(cancellationToken);
```
