# Algocura Search API

The Search API provides a unified read model across the Algocura Drug
Discovery Platform.

It consumes supported integration events, converts them into searchable
documents, maintains local projections, and exposes filtering and
free-text search endpoints.

## Searchable document types

- Publication
- Target
- Chemical Design
- Compound
- Screening
- Assay
- Prediction
- Workflow
- Notification

## Prototype limitation

The current implementation uses SQL Server `Contains` filters and local
search projections. It does not provide stemming, fuzzy matching,
semantic ranking, vector search, or distributed indexing.

For production-scale search, replace or augment the persistence layer
with Azure AI Search, Elasticsearch, OpenSearch, or another approved
search platform while preserving the event-driven projection model.

## Local URLs

- `http://localhost:7009/`
- `http://localhost:7009/swagger`
- `http://localhost:7009/redoc`
- `http://localhost:7009/openapi/v1.json`
- `http://localhost:7009/health`

## Routes

- `GET /api/v1/search`
- `GET /api/v1/search-documents/{id}`
- `POST /api/v1/search-documents/{id}/archive`
- `POST /api/v1/search-documents/{id}/restore`
- `POST /api/v1/search/synchronization`

## Events published

- `SearchDocumentIndexed`
- `SearchIndexSnapshot`

## Migrations

```powershell
Add-Migration InitialCreate `
  -Context SearchDbContext `
  -OutputDir Data/Migrations

Update-Database `
  -Context SearchDbContext
```

Then replace `EnsureCreatedAsync()` with `MigrateAsync()`.
