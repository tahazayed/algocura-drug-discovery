# Algocura.AssayManagement.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 24.

## Original documented intent

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

---

## Shared Platform Messaging

`Algocura.AssayManagement.Api` uses `Algocura.Platform.Messaging` for common
Azure Service Bus transport and consumer infrastructure.

The API no longer owns local implementations of:

```text
IEventPublisher
ServiceBusEventPublisher
ServiceBusClient registration
ServiceBusOptions
Service Bus processor lifecycle
```

Published Assay Management events flow through:

```text
AssayCreated / AssayCompleted / AssaySnapshot
        ↓
Algocura.Platform.Messaging.Publishing.IEventPublisher
        ↓
ServiceBusEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
ServiceBusMessagePublisher
        ↓
Azure Service Bus
```

Audit events use the same underlying transport:

```text
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
Azure Service Bus
```

### Shared event contract

Assay Management events now inherit from the platform
`IntegrationEvent` contract through:

```text
AssayManagementIntegrationEvent
        ↓
Algocura.Platform.Messaging.Events.IntegrationEvent
```

`AssayManagementIntegrationEvent` preserves the service source:

```text
Algocura.AssayManagement.Api
```

Domain-specific events remain owned by this API:

```text
AssayCreated
AssayCompleted
AssaySnapshot
```

The consumed `CompletedScreeningSnapshot` remains a local incoming contract
owned by the Assay Management integration boundary.

### Shared consumer base

`ScreeningEventsConsumer` now inherits from:

```text
ServiceBusConsumerBase
```

The shared base owns:

```text
ServiceBusProcessor creation
Topic/subscription configuration
Processor startup
Processor shutdown
Processor disposal
Cancellation handling
Common processor error logging
Event-type resolution
Source resolution
Event-ID resolution
```

`ScreeningEventsConsumer` continues to own the Assay Management-specific
processing of `CompletedScreeningSnapshot`.

### Registration

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

builder.Services.AddHostedService<
    ScreeningEventsConsumer>();
```

`AddAlgocuraPlatformMessaging(...)` already registers:

```text
ServiceBusClient
IPlatformMessagePublisher -> ServiceBusMessagePublisher
IEventPublisher           -> ServiceBusEventPublisher
ServiceBusAuditEventPublisher
```

Do not register another API-local `IEventPublisher`,
`ServiceBusEventPublisher`, or `ServiceBusClient`.

### Service Bus configuration

```json
"ServiceBus": {
  "UseEmulator": true,
  "ConnectionString": "...",
  "FullyQualifiedNamespace": "",
  "TopicName": "algocura-events",
  "SubscriptionName": "assay-management-api",
  "DefaultMessageTimeToLiveDays": 7,
  "MaxConcurrentCalls": 4,
  "PrefetchCount": 0,
  "MaxAutoLockRenewalMinutes": 5
}
```

`SubscriptionName` is required because Assay Management consumes completed
screening events.

### Auditing configuration

```json
"Auditing": {
  "ServiceName": "Algocura.AssayManagement.Api",
  "PublishEntityAuditEvents": true,
  "StampCreatedAndUpdatedFields": true,
  "IncludeUnchangedPropertiesOnCreate": true,
  "IncludeUnchangedPropertiesOnDelete": true,
  "IgnoreAuditInfrastructureEntities": true
}
```



## Project definition (authoritative build metadata)

Source: `Algocura.AssayManagement.Api/Algocura.AssayManagement.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<RootNamespace>Algocura.AssayManagement.Api</RootNamespace>
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
| `Algocura.AssayManagement.Api/Algocura.AssayManagement.Api.csproj` | 31 |
| `Algocura.AssayManagement.Api/Contracts/AssayContracts.cs` | 90 |
| `Algocura.AssayManagement.Api/Controllers/AssaySynchronizationController.cs` | 152 |
| `Algocura.AssayManagement.Api/Controllers/AssaysController.cs` | 451 |
| `Algocura.AssayManagement.Api/Controllers/CompletedScreeningsController.cs` | 55 |
| `Algocura.AssayManagement.Api/Data/AssayManagementDbContext.cs` | 200 |
| `Algocura.AssayManagement.Api/Data/AssayManagementDbContextFactory.cs` | 25 |
| `Algocura.AssayManagement.Api/Data/AssayManagementSeeder.cs` | 678 |
| `Algocura.AssayManagement.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.AssayManagement.Api/Data/Migrations/20260113033817_InitialCreate.Designer.cs` | 319 |
| `Algocura.AssayManagement.Api/Data/Migrations/20260113033817_InitialCreate.cs` | 187 |
| `Algocura.AssayManagement.Api/Data/Migrations/20260806015749_AddAuditMetadata.Designer.cs` | 438 |
| `Algocura.AssayManagement.Api/Data/Migrations/20260806015749_AddAuditMetadata.cs` | 445 |
| `Algocura.AssayManagement.Api/Data/Migrations/AssayManagementDbContextModelSnapshot.cs` | 435 |
| `Algocura.AssayManagement.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.AssayManagement.Api/Domain/Entities.cs` | 278 |
| `Algocura.AssayManagement.Api/Events/IntegrationEvents.cs` | 101 |
| `Algocura.AssayManagement.Api/Mapping/AssayMappings.cs` | 94 |
| `Algocura.AssayManagement.Api/Messaging/Consumers/ScreeningEventsConsumer.cs` | 209 |
| `Algocura.AssayManagement.Api/Program.cs` | 413 |
| `Algocura.AssayManagement.Api/Properties/launchSettings.json` | 23 |
| `Algocura.AssayManagement.Api/README.md` | 236 |
| `Algocura.AssayManagement.Api/Services/AssayResultCalculator.cs` | 141 |
| `Algocura.AssayManagement.Api/appsettings.json` | 45 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.AssayManagement.Api/Controllers/AssaysController.cs`

- L14: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L16: `[Route("api/v1/assays")]`
- L24: `[HttpGet]`
- L56: `[HttpGet("{id:guid}")]`
- L70: `[HttpPost]`
- L142: `[HttpPost("{id:guid}/measurements")]`
- L235: `[HttpPost("{id:guid}/complete")]`
- L355: `[HttpPost("{id:guid}/publish")]`

### `Algocura.AssayManagement.Api/Controllers/AssaySynchronizationController.cs`

- L10: `[Authorize(Roles = "Service,Administrator")]`
- L12: `[Route("api/v1/assay-management/synchronization")]`
- L19: `[HttpPost]`
- L83: `[HttpPost("{id:guid}")]`

### `Algocura.AssayManagement.Api/Controllers/CompletedScreeningsController.cs`

- L10: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L12: `[Route("api/v1/completed-screenings")]`
- L17: `[HttpGet]`
- L38: `[HttpGet("{id:guid}")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.AssayManagement.Api/Contracts/AssayContracts.cs`
Types: `CreateAssayRequest`, `AddAssayMeasurementRequest`, `CompleteAssayRequest`, `ScreeningResultResponse`, `CompletedScreeningResponse`, `AssayMeasurementResponse`, `AssayResultResponse`, `AssayResponse`

### `Algocura.AssayManagement.Api/Data/AssayManagementDbContext.cs`
Types: `AssayManagementDbContext`
- L11: `public DbSet<CompletedScreeningProjection> Screenings =>`
- L14: `public DbSet<CompletedScreeningResultProjection> ScreeningResults =>`
- L17: `public DbSet<Assay> Assays =>`
- L20: `public DbSet<AssayMeasurement> AssayMeasurements =>`
- L23: `public DbSet<AssayResult> AssayResults =>`
- L28: `modelBuilder.Entity<CompletedScreeningProjection>(entity =>`
- L30: `entity.HasKey(x => x.ScreeningId);`
- L32: `entity.Property(x => x.ScreeningId)`
- L35: `entity.HasMany(x => x.Results)`
- L40: `entity.HasMany(x => x.Assays)`
- L46: `modelBuilder.Entity<CompletedScreeningResultProjection>(entity =>`
- L48: `entity.HasKey(x => x.Id);`
- L50: `entity.Property(x => x.Id)`
- L53: `entity.Property(x => x.Score)`
- L56: `entity.HasIndex(x => new`
- L63: `modelBuilder.Entity<Assay>(entity =>`
- L65: `entity.HasKey(x => x.Id);`
- L67: `entity.Property(x => x.Id)`
- L70: `entity.Property(x => x.ActivityThreshold)`
- L73: `entity.Property(x => x.ZPrimeFactor)`
- L76: `entity.HasIndex(x => x.ScreeningId);`
- L77: `entity.HasIndex(x => x.Status);`
- L79: `entity.HasMany(x => x.Measurements)`
- L84: `entity.HasMany(x => x.Results)`
- L90: `modelBuilder.Entity<AssayMeasurement>(entity =>`
- L92: `entity.HasKey(x => x.Id);`
- L94: `entity.Property(x => x.Id)`
- L97: `entity.Property(x => x.Concentration)`
- L100: `entity.Property(x => x.Value)`
- L103: `entity.HasIndex(x => new`
- L112: `modelBuilder.Entity<AssayResult>(entity =>`
- L114: `entity.HasKey(x => x.Id);`
- L116: `entity.Property(x => x.Id)`
- L119: `entity.Property(x => x.MeanValue)`
- L122: `entity.Property(x => x.StandardDeviation)`
- L125: `entity.Property(x => x.Ic50)`
- L128: `entity.Property(x => x.Ec50)`
- L131: `entity.Property(x => x.Ki)`
- L134: `entity.Property(x => x.Kd)`
- L137: `entity.Property(x => x.ConfidenceScore)`
- L140: `entity.HasIndex(x => new`
- L155: `modelBuilder.Model`
- L167: `modelBuilder.Entity(`
- L170: `entity.Property<Guid?>(`
- L175: `entity.Property<Guid?>(`
- L180: `entity.Property<DateTimeOffset>(`
- L185: `entity.Property<DateTimeOffset>(`
- L190: `entity.HasIndex(`
- L194: `entity.HasIndex(`

### `Algocura.AssayManagement.Api/Data/AssayManagementDbContextFactory.cs`
Types: `AssayManagementDbContextFactory`

### `Algocura.AssayManagement.Api/Data/AssayManagementSeeder.cs`
Types: `AssayManagementSeeder`, `is`

### `Algocura.AssayManagement.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.AssayManagement.Api/Data/Migrations/20260113033817_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L96: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayMeasurement", b =>`
- L139: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayResult", b =>`
- L198: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningProjection", b =>`
- L230: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningResultProjection", b =>`
- L259: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L270: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayMeasurement", b =>`
- L281: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayResult", b =>`
- L292: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningResultProjection", b =>`
- L303: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L310: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningProjection", b =>`

### `Algocura.AssayManagement.Api/Data/Migrations/20260113033817_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.AssayManagement.Api/Data/Migrations/20260806015749_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L121: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayMeasurement", b =>`
- L189: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayResult", b =>`
- L273: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningProjection", b =>`
- L327: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningResultProjection", b =>`
- L378: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L389: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayMeasurement", b =>`
- L400: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayResult", b =>`
- L411: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningResultProjection", b =>`
- L422: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L429: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningProjection", b =>`

### `Algocura.AssayManagement.Api/Data/Migrations/20260806015749_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.AssayManagement.Api/Data/Migrations/AssayManagementDbContextModelSnapshot.cs`
Types: `AssayManagementDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L118: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayMeasurement", b =>`
- L186: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayResult", b =>`
- L270: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningProjection", b =>`
- L324: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningResultProjection", b =>`
- L375: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L386: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayMeasurement", b =>`
- L397: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.AssayResult", b =>`
- L408: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningResultProjection", b =>`
- L419: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.Assay", b =>`
- L426: `modelBuilder.Entity("Algocura.AssayManagement.Api.Domain.CompletedScreeningProjection", b =>`

### `Algocura.AssayManagement.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.AssayManagement.Api/Messaging/Consumers/ScreeningEventsConsumer.cs`

### `Algocura.AssayManagement.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<AssayManagementDbContext>(`
- L140 registration: `builder.Services.AddScoped<`
- L235 registration: `app.UseAuthentication();`
- L236 registration: `app.UseAuthorization();`
- L301 registration: `app.MapControllers();`

### `Algocura.AssayManagement.Api/Services/AssayResultCalculator.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
