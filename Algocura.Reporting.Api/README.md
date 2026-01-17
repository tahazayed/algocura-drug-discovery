# Algocura Reporting API

The Reporting API provides read-optimized reporting projections,
report definitions, report execution, persisted metrics, platform
summaries, and reporting synchronization.

## Endpoints

```text
GET  /api/v1/report-definitions
GET  /api/v1/report-definitions/{id}
POST /api/v1/report-definitions
POST /api/v1/report-definitions/{id}/runs

GET  /api/v1/report-runs
GET  /api/v1/report-runs/{id}

GET  /api/v1/reporting/summary
POST /api/v1/reporting/synchronization
```

## Seed data

The seed process creates:

- Platform Summary report definition
- Discovery Funnel report definition
- Eight reporting projections
- Two completed report runs
- Five report metrics

All seeded records use the system identity:

```text
00000000-0000-0000-0000-000000000001
```

## Migration

```powershell
Add-Migration InitialCreate `
    -Context ReportingDbContext `
    -OutputDir Data/Migrations

Update-Database `
    -Context ReportingDbContext
```

## Docker Compose

```yaml
reporting-api:
  image: algocura/reporting-api:dev
  build:
    context: .
    dockerfile: Algocura.Reporting.Api/Dockerfile
  ports:
    - "7010:8080"
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__ReportingDatabase: >
      Server=sqlserver,1433;
      Database=AlgocuraReporting;
      User Id=sa;
      Password=DontLogin!Passw0rd;
      TrustServerCertificate=True
    ServiceBus__UseEmulator: "true"
    ServiceBus__ConnectionString: >
      Endpoint=sb://servicebus-emulator;
      SharedAccessKeyName=RootManageSharedAccessKey;
      SharedAccessKey=SAS_KEY_VALUE;
      UseDevelopmentEmulator=true;
    ServiceBus__TopicName: algocura-events
    ServiceBus__SubscriptionName: reporting-api
```
