# Algocura.Reporting.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 18.

## Original documented intent

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



## Project definition (authoritative build metadata)

Source: `Algocura.Reporting.Api/Algocura.Reporting.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
	<ItemGroup>
		<PackageReference Include="Azure.Identity" Version="1.*" />
		<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.*" />
		<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.10" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.10" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.OpenApi" Version="2.7.5" />
		<PackageReference Include="Swashbuckle.AspNetCore.ReDoc" Version="10.2.3" />
		<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.2.3" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Algocura.Platform.Auditing\Algocura.Platform.Auditing.csproj" />
		<ProjectReference Include="..\Algocura.Platform.Messaging\Algocura.Platform.Messaging.csproj" />
		<ProjectReference Include="..\Algocura.Platform.Security\Algocura.Platform.Security.csproj" />
	</ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Reporting.Api/Algocura.Reporting.Api.csproj` | 25 |
| `Algocura.Reporting.Api/Contracts/Contracts.cs` | 52 |
| `Algocura.Reporting.Api/Controllers/ReportingControllers.cs` | 294 |
| `Algocura.Reporting.Api/Controllers/ReportingSynchronizationController.cs` | 59 |
| `Algocura.Reporting.Api/Data/DatabaseInitializer.cs` | 57 |
| `Algocura.Reporting.Api/Data/Migrations/20260807030327_InitialCreate.Designer.cs` | 347 |
| `Algocura.Reporting.Api/Data/Migrations/20260807030327_InitialCreate.cs` | 242 |
| `Algocura.Reporting.Api/Data/Migrations/ReportingDbContextModelSnapshot.cs` | 344 |
| `Algocura.Reporting.Api/Data/ReportingDbContext.cs` | 70 |
| `Algocura.Reporting.Api/Data/ReportingSeeder.cs` | 246 |
| `Algocura.Reporting.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.Reporting.Api/Domain/Entities.cs` | 91 |
| `Algocura.Reporting.Api/Messaging/Messaging.cs` | 26 |
| `Algocura.Reporting.Api/Program.cs` | 396 |
| `Algocura.Reporting.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Reporting.Api/README.md` | 104 |
| `Algocura.Reporting.Api/Services/ReportExecutionService.cs` | 205 |
| `Algocura.Reporting.Api/appsettings.json` | 42 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Reporting.Api/Controllers/ReportingControllers.cs`

- L11: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L13: `[Route("api/v1/report-definitions")]`
- L19: `[HttpGet]`
- L39: `[HttpGet("{id:guid}")]`
- L60: `[HttpPost]`
- L102: `[HttpPut("{id:guid}")]`
- L136: `[HttpDelete("{id:guid}")]`
- L155: `[HttpPost("{id:guid}/runs")]`
- L185: `private static ReportRunResponse Map(ReportRun run) =>`
- L202: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L204: `[Route("api/v1/report-runs")]`
- L209: `[HttpGet]`
- L222: `[HttpGet("{id:guid}")]`
- L237: `private static ReportRunResponse Map(ReportRun run) =>`
- L254: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L256: `[Route("api/v1/reporting/summary")]`
- L261: `[HttpGet]`

### `Algocura.Reporting.Api/Controllers/ReportingSynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/reporting/synchronization")]`
- L19: `[HttpPost]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Reporting.Api/Contracts/Contracts.cs`
Types: `CreateReportDefinitionRequest`, `UpdateReportDefinitionRequest`, `RunReportRequest`, `ReportDefinitionResponse`, `ReportMetricResponse`, `ReportRunResponse`, `ReportingSummaryResponse`

### `Algocura.Reporting.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.Reporting.Api/Data/Migrations/20260807030327_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportDefinition", b =>`
- L89: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportMetric", b =>`
- L141: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportRun", b =>`
- L209: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportingProjection", b =>`
- L313: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportMetric", b =>`
- L324: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportRun", b =>`
- L335: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportDefinition", b =>`
- L340: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportRun", b =>`

### `Algocura.Reporting.Api/Data/Migrations/20260807030327_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.Reporting.Api/Data/Migrations/ReportingDbContextModelSnapshot.cs`
Types: `ReportingDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportDefinition", b =>`
- L86: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportMetric", b =>`
- L138: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportRun", b =>`
- L206: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportingProjection", b =>`
- L310: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportMetric", b =>`
- L321: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportRun", b =>`
- L332: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportDefinition", b =>`
- L337: `modelBuilder.Entity("Algocura.Reporting.Api.Domain.ReportRun", b =>`

### `Algocura.Reporting.Api/Data/ReportingDbContext.cs`
Types: `ReportingDbContext`
- L9: `public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();`
- L10: `public DbSet<ReportRun> ReportRuns => Set<ReportRun>();`
- L11: `public DbSet<ReportMetric> ReportMetrics => Set<ReportMetric>();`
- L12: `public DbSet<ReportingProjection> ReportingProjections => Set<ReportingProjection>();`
- L16: `modelBuilder.Entity<ReportDefinition>(e =>`
- L27: `modelBuilder.Entity<ReportRun>(e =>`
- L37: `modelBuilder.Entity<ReportMetric>(e =>`
- L45: `modelBuilder.Entity<ReportingProjection>(e =>`
- L58: `foreach (var entityType in modelBuilder.Model.GetEntityTypes()`
- L61: `var e = modelBuilder.Entity(entityType.ClrType);`

### `Algocura.Reporting.Api/Data/ReportingSeeder.cs`
Types: `ReportingSeeder`

### `Algocura.Reporting.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Reporting.Api/Messaging/Messaging.cs`

### `Algocura.Reporting.Api/Program.cs`
- L21 registration: `builder.Services.AddHealthChecks();`
- L34 registration: `builder.Services.AddCors(`
- L102 registration: `builder.Services.AddDbContext<ReportingDbContext>(`
- L139 registration: `builder.Services.AddScoped<`
- L221 registration: `app.UseAuthentication();`
- L222 registration: `app.UseAuthorization();`
- L287 registration: `app.MapControllers();`

### `Algocura.Reporting.Api/Services/ReportExecutionService.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
