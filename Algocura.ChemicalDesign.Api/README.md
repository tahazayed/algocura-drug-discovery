# Algocura Chemical Design API

The service is the third stage in the workflow:

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

It consumes `TargetDiscoverySnapshot`, manages design projects and query
structures, executes prototype structure searches, ranks candidate
structures, stores synthesis routes, and publishes selected compounds.

## Prototype limitation

`DeterministicStructureSearchEngine` is not ChemAxon and is not a
chemically validated search engine. It uses normalized string and token
comparison only to validate the service workflow. Replace it with a
ChemAxon adapter or another validated cheminformatics engine before
scientific use.

## Local URLs

- `http://localhost:7003/`
- `http://localhost:7003/swagger`
- `http://localhost:7003/redoc`
- `http://localhost:7003/openapi/v1.json`
- `http://localhost:7003/health`

## Main routes

- `GET /api/v1/selected-targets`
- `GET /api/v1/selected-targets/{id}`
- `POST /api/v1/design-projects`
- `GET /api/v1/design-projects/{id}`
- `POST /api/v1/design-projects/{id}/query-structures`
- `POST /api/v1/design-projects/{projectId}/structure-searches`
- `GET /api/v1/structure-searches/{id}`
- `POST /api/v1/structure-searches/{id}/run`
- `GET /api/v1/candidates/{id}`
- `POST /api/v1/candidates/{id}/select-for-registration`
- `POST /api/v1/candidates/{id}/synthesis-routes`
- `POST /api/v1/chemical-design/synchronization`
- `POST /api/v1/chemical-design/synchronization/{id}`

## Events

Consumed:

- `TargetDiscoverySnapshot`

Published:

- `ChemicalDesignProjectCreated`
- `StructureSearchCompleted`
- `CandidateSelectedForRegistration`
- `ChemicalDesignSnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()`. Generate the initial migration:

```powershell
dotnet ef migrations add InitialCreate `
  --project .\Algocura.ChemicalDesign.Api\Algocura.ChemicalDesign.Api.csproj `
  --startup-project .\Algocura.ChemicalDesign.Api\Algocura.ChemicalDesign.Api.csproj `
  --context ChemicalDesignDbContext `
  --output-dir Migrations
```

Then replace `EnsureCreatedAsync()` with `MigrateAsync()`.
