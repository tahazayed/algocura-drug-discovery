# Algocura.Workflow.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 23.

## Original documented intent

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

Source: `Algocura.Workflow.Api/Algocura.Workflow.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<RootNamespace>Algocura.Workflow.Api</RootNamespace>
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
| `Algocura.Workflow.Api/Algocura.Workflow.Api.csproj` | 27 |
| `Algocura.Workflow.Api/Contracts/WorkflowContracts.cs` | 39 |
| `Algocura.Workflow.Api/Controllers/WorkflowSynchronizationController.cs` | 159 |
| `Algocura.Workflow.Api/Controllers/WorkflowsController.cs` | 244 |
| `Algocura.Workflow.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.Workflow.Api/Data/Migrations/20260113043822_InitialCreate.Designer.cs` | 186 |
| `Algocura.Workflow.Api/Data/Migrations/20260113043822_InitialCreate.cs` | 118 |
| `Algocura.Workflow.Api/Data/Migrations/20260806020404_AddAuditMetadata.Designer.cs` | 252 |
| `Algocura.Workflow.Api/Data/Migrations/20260806020404_AddAuditMetadata.cs` | 255 |
| `Algocura.Workflow.Api/Data/Migrations/WorkflowDbContextModelSnapshot.cs` | 249 |
| `Algocura.Workflow.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.Workflow.Api/Data/WorkflowDbContext.cs` | 123 |
| `Algocura.Workflow.Api/Data/WorkflowDbContextFactory.cs` | 23 |
| `Algocura.Workflow.Api/Data/WorkflowSeeder.cs` | 462 |
| `Algocura.Workflow.Api/Domain/WorkflowEntities.cs` | 158 |
| `Algocura.Workflow.Api/Events/IntegrationEvents.cs` | 72 |
| `Algocura.Workflow.Api/Mapping/WorkflowMappings.cs` | 38 |
| `Algocura.Workflow.Api/Messaging/Consumers/PlatformWorkflowEventsConsumer.cs` | 389 |
| `Algocura.Workflow.Api/Program.cs` | 402 |
| `Algocura.Workflow.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Workflow.Api/README.md` | 115 |
| `Algocura.Workflow.Api/Services/WorkflowStateMachine.cs` | 200 |
| `Algocura.Workflow.Api/appsettings.json` | 45 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Workflow.Api/Controllers/WorkflowsController.cs`

- L14: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L16: `[Route("api/v1/workflows")]`
- L23: `[HttpGet]`
- L57: `[HttpGet("{id:guid}")]`
- L72: `[HttpPost]`
- L138: `[HttpPost("{id:guid}/start")]`
- L163: `[HttpPost("{id:guid}/stages/{stage}/fail")]`
- L192: `[HttpPost("{id:guid}/cancel")]`

### `Algocura.Workflow.Api/Controllers/WorkflowSynchronizationController.cs`

- L10: `[Authorize(Roles = "Service,Administrator")]`
- L12: `[Route("api/v1/workflow/synchronization")]`
- L19: `[HttpPost]`
- L81: `[HttpPost("{id:guid}")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Workflow.Api/Contracts/WorkflowContracts.cs`
Types: `CreateWorkflowRequest`, `FailWorkflowStageRequest`, `WorkflowStageResponse`, `DiscoveryWorkflowResponse`

### `Algocura.Workflow.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.Workflow.Api/Data/Migrations/20260113043822_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.DiscoveryWorkflow", b =>`
- L78: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowEventReceipt", b =>`
- L109: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowStageExecution", b =>`
- L155: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowEventReceipt", b =>`
- L166: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowStageExecution", b =>`
- L177: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.DiscoveryWorkflow", b =>`

### `Algocura.Workflow.Api/Data/Migrations/20260113043822_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.Workflow.Api/Data/Migrations/20260806020404_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.DiscoveryWorkflow", b =>`
- L103: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowEventReceipt", b =>`
- L156: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowStageExecution", b =>`
- L221: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowEventReceipt", b =>`
- L232: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowStageExecution", b =>`
- L243: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.DiscoveryWorkflow", b =>`

### `Algocura.Workflow.Api/Data/Migrations/20260806020404_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.Workflow.Api/Data/Migrations/WorkflowDbContextModelSnapshot.cs`
Types: `WorkflowDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.DiscoveryWorkflow", b =>`
- L100: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowEventReceipt", b =>`
- L153: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowStageExecution", b =>`
- L218: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowEventReceipt", b =>`
- L229: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.WorkflowStageExecution", b =>`
- L240: `modelBuilder.Entity("Algocura.Workflow.Api.Domain.DiscoveryWorkflow", b =>`

### `Algocura.Workflow.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

### `Algocura.Workflow.Api/Data/WorkflowDbContext.cs`
Types: `WorkflowDbContext`
- L11: `public DbSet<DiscoveryWorkflow> Workflows =>`
- L14: `public DbSet<WorkflowStageExecution> WorkflowStages =>`
- L17: `public DbSet<WorkflowEventReceipt> WorkflowEventReceipts =>`
- L22: `modelBuilder.Entity<DiscoveryWorkflow>(entity =>`
- L24: `entity.HasKey(x => x.Id);`
- L26: `entity.Property(x => x.Id)`
- L29: `entity.HasIndex(x => x.Status);`
- L30: `entity.HasIndex(x => x.DiseaseName);`
- L32: `entity.HasMany(x => x.Stages)`
- L37: `entity.HasMany(x => x.EventReceipts)`
- L43: `modelBuilder.Entity<WorkflowStageExecution>(entity =>`
- L45: `entity.HasKey(x => x.Id);`
- L47: `entity.Property(x => x.Id)`
- L50: `entity.HasIndex(x => new`
- L57: `modelBuilder.Entity<WorkflowEventReceipt>(entity =>`
- L59: `entity.HasKey(x => x.EventId);`
- L61: `entity.Property(x => x.EventId)`
- L64: `entity.HasIndex(x => new`
- L79: `modelBuilder.Model`
- L91: `modelBuilder.Entity(`
- L94: `entity.Property<Guid?>(`
- L99: `entity.Property<Guid?>(`
- L104: `entity.Property<DateTimeOffset>(`
- L109: `entity.Property<DateTimeOffset>(`
- L114: `entity.HasIndex(`
- L118: `entity.HasIndex(`

### `Algocura.Workflow.Api/Data/WorkflowDbContextFactory.cs`
Types: `WorkflowDbContextFactory`

### `Algocura.Workflow.Api/Data/WorkflowSeeder.cs`
Types: `WorkflowSeeder`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Workflow.Api/Messaging/Consumers/PlatformWorkflowEventsConsumer.cs`

### `Algocura.Workflow.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<WorkflowDbContext>(`
- L140 registration: `builder.Services.AddScoped<`
- L224 registration: `app.UseAuthentication();`
- L225 registration: `app.UseAuthorization();`
- L290 registration: `app.MapControllers();`

### `Algocura.Workflow.Api/Services/WorkflowStateMachine.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
