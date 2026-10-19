# Algocura.Audit.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 19.

## Original documented intent

# Algocura Audit API 

The Audit API is the centralized, read-only audit-query service for the
Algocura Drug Discovery Platform.

It consumes `EntityAuditEvent` messages emitted by platform services and
stores them in its own SQL Server database for traceability, investigation,
compliance support, and operational review.

## Responsibilities

- Consume `EntityAuditEvent`
- Persist immutable audit history
- Deduplicate events by `EventId`
- Search by service, entity, action, user, correlation, workflow, and date
- Retrieve a single audit event
- Produce aggregate audit summaries
- Expose audit synchronization metadata
- Preserve previous/current/changed-value JSON
- Preserve actor, trace, correlation, IP, and user-agent metadata
- Seed prototype audit records
- Use Algocura shared auditing for local persistence stamping without
  publishing audit events about the audit records themselves

## Endpoints

```text
GET /api/v1/audit
GET /api/v1/audit/{eventId}
GET /api/v1/audit/summary
GET /api/v1/audit/synchronization

GET /health
GET /swagger
GET /redoc
GET /openapi/v1.json
```

## Query example

```text
GET /api/v1/audit?
    serviceName=Algocura.Screening.Api&
    entityType=ScreeningJob&
    action=Completed&
    workflowId=71000000-0000-0000-0000-000000000001
```

## EntityAuditEvent

The Audit API wire contract follows the shared
`Algocura.Platform.Auditing.Events.EntityAuditEvent` schema:

```text
EventId
OccurredAtUtc
ServiceName
EntityType
EntityId
Action
UserId
Subject
UserDisplayName
UserEmail
ActorType
PreviousValuesJson
CurrentValuesJson
ChangedPropertiesJson
CorrelationId
WorkflowId
TraceId
IpAddress
UserAgent
EventType
EventVersion
```

## Audit feedback-loop protection

The Audit API registers:

```csharp
options.PublishEntityAuditEvents = false;
options.IgnoreAuditInfrastructureEntities = true;
```

Therefore its own `AuditRecord` inserts do not create another
`EntityAuditEvent`.

## Seed data

The seeder inserts deterministic prototype audit events covering:

```text
Literature Intelligence
Target Discovery
Chemical Design
Compound Catalog
Screening
Assay Management
AI Discovery
Workflow
Notification
Reporting
```

All seed persistence metadata uses:

```text
00000000-0000-0000-0000-000000000001
```

## Database migration

```powershell
Add-Migration InitialCreate `
    -Context AuditDbContext `
    -OutputDir Data/Migrations

Update-Database `
    -Context AuditDbContext
```

## Service Bus

Add the following subscription to `algocura-events`:

```text
audit-api
```

The subscription should receive `EntityAuditEvent` messages.

## Important platform integration

The Audit API can only receive entity-audit messages after the producing
services use a real `IAuditEventPublisher`.

Those services currently need to change from:

```csharp
builder.Services.AddAlgocuraAuditing(options =>
{
    options.PublishEntityAuditEvents = false;
});
```

to the generic registration that supplies a Service Bus implementation:

```csharp
builder.Services.AddAlgocuraAuditing<ServiceBusAuditEventPublisher>(
    options =>
    {
        options.ServiceName = "Algocura.Screening.Api";
        options.StampCreatedAndUpdatedFields = true;
        options.PublishEntityAuditEvents = true;
    });
```

The publisher should publish `EntityAuditEvent` to `algocura-events`
with:

```text
Subject = EntityAuditEvent
ApplicationProperties["eventType"] = EntityAuditEvent
```

A shared publisher implementation should preferably live in
`Algocura.Platform.Auditing` or another shared platform messaging library
so every microservice uses the same audit-event publishing contract.

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



## Project definition (authoritative build metadata)

Source: `Algocura.Audit.Api/Algocura.Audit.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <RootNamespace>Algocura.Audit.Api</RootNamespace>
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
| `Algocura.Audit.Api/Algocura.Audit.Api.csproj` | 29 |
| `Algocura.Audit.Api/Contracts/AuditContracts.cs` | 37 |
| `Algocura.Audit.Api/Controllers/AuditController.cs` | 237 |
| `Algocura.Audit.Api/Controllers/AuditSynchronizationController.cs` | 42 |
| `Algocura.Audit.Api/Data/AuditDbContext.cs` | 145 |
| `Algocura.Audit.Api/Data/AuditSeeder.cs` | 224 |
| `Algocura.Audit.Api/Data/DatabaseInitializer.cs` | 82 |
| `Algocura.Audit.Api/Data/Migrations/20260117045838_InitialCreate.Designer.cs` | 154 |
| `Algocura.Audit.Api/Data/Migrations/20260117045838_InitialCreate.cs` | 97 |
| `Algocura.Audit.Api/Data/Migrations/AuditDbContextModelSnapshot.cs` | 151 |
| `Algocura.Audit.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.Audit.Api/Domain/AuditRecord.cs` | 79 |
| `Algocura.Audit.Api/Messaging/EntityAuditEventsConsumer.cs` | 138 |
| `Algocura.Audit.Api/Messaging/IncomingEntityAuditEvent.cs` | 60 |
| `Algocura.Audit.Api/Program.cs` | 397 |
| `Algocura.Audit.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Audit.Api/README.md` | 199 |
| `Algocura.Audit.Api/Services/AuditIngestionService.cs` | 150 |
| `Algocura.Audit.Api/appsettings.json` | 42 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Audit.Api/Controllers/AuditController.cs`

- L9: `[Authorize(Roles = "Reviewer,Administrator")]`
- L11: `[Route("api/v1/audit")]`
- L16: `[HttpGet]`
- L139: `[HttpGet("{eventId:guid}")]`
- L180: `[HttpGet("summary")]`

### `Algocura.Audit.Api/Controllers/AuditSynchronizationController.cs`

- L8: `[Authorize(Roles = "Service,Administrator")]`
- L10: `[Route("api/v1/audit/synchronization")]`
- L15: `[HttpGet]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Audit.Api/Contracts/AuditContracts.cs`
Types: `AuditRecordResponse`, `AuditSearchResponse`, `AuditSummaryResponse`

### `Algocura.Audit.Api/Data/AuditDbContext.cs`
Types: `AuditDbContext`
- L11: `public DbSet<AuditRecord> AuditRecords =>`
- L17: `modelBuilder.Entity<AuditRecord>(entity =>`
- L19: `entity.ToTable("AuditRecords");`
- L21: `entity.HasKey(x => x.EventId);`
- L23: `entity.Property(x => x.ServiceName)`
- L27: `entity.Property(x => x.EntityType)`
- L31: `entity.Property(x => x.EntityId)`
- L35: `entity.Property(x => x.Action)`
- L39: `entity.Property(x => x.Subject)`
- L42: `entity.Property(x => x.UserDisplayName)`
- L45: `entity.Property(x => x.UserEmail)`
- L48: `entity.Property(x => x.ActorType)`
- L54: `entity.Property(x => x.PreviousValuesJson)`
- L57: `entity.Property(x => x.CurrentValuesJson)`
- L60: `entity.Property(x => x.ChangedPropertiesJson)`
- L63: `entity.Property(x => x.CorrelationId)`
- L66: `entity.Property(x => x.WorkflowId)`
- L69: `entity.Property(x => x.TraceId)`
- L72: `entity.Property(x => x.IpAddress)`
- L75: `entity.Property(x => x.UserAgent)`
- L78: `entity.Property(x => x.EventType)`
- L82: `entity.Property(x => x.EventVersion)`
- L86: `entity.HasIndex(x => x.OccurredAtUtc);`
- L88: `entity.HasIndex(x => x.ServiceName);`
- L90: `entity.HasIndex(x => new`
- L96: `entity.HasIndex(x => x.UserId);`
- L98: `entity.HasIndex(x => x.CorrelationId);`
- L100: `entity.HasIndex(x => x.WorkflowId);`
- L110: `modelBuilder.Model`
- L120: `modelBuilder.Entity(entityType.ClrType);`
- L122: `entity.Property<Guid?>(`
- L126: `entity.Property<Guid?>(`
- L130: `entity.Property<DateTimeOffset>(`
- L134: `entity.Property<DateTimeOffset>(`
- L138: `entity.HasIndex(`
- L141: `entity.HasIndex(`

### `Algocura.Audit.Api/Data/AuditSeeder.cs`
Types: `AuditSeeder`

### `Algocura.Audit.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.Audit.Api/Data/Migrations/20260117045838_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.Audit.Api.Domain.AuditRecord", b =>`

### `Algocura.Audit.Api/Data/Migrations/20260117045838_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.Audit.Api/Data/Migrations/AuditDbContextModelSnapshot.cs`
Types: `AuditDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.Audit.Api.Domain.AuditRecord", b =>`

### `Algocura.Audit.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Audit.Api/Messaging/EntityAuditEventsConsumer.cs`

### `Algocura.Audit.Api/Messaging/IncomingEntityAuditEvent.cs`

### `Algocura.Audit.Api/Program.cs`
- L21 registration: `builder.Services.AddHealthChecks();`
- L34 registration: `builder.Services.AddCors(`
- L91 registration: `builder.Services.AddDbContext<AuditDbContext>(`
- L128 registration: `builder.Services.AddScoped<`
- L212 registration: `app.UseAuthentication();`
- L213 registration: `app.UseAuthorization();`
- L278 registration: `app.MapControllers();`

### `Algocura.Audit.Api/Services/AuditIngestionService.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
