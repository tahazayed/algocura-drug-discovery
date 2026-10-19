# Algocura.AIDiscovery.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 29.

## Original documented intent

# Algocura AI Discovery API

The AI Discovery API manages compound and screening projections, prediction
jobs, deterministic prototype scoring, candidate prioritization, and
downstream integration events.

## Shared Platform Messaging

The API uses `Algocura.Platform.Messaging` for common Azure Service Bus
infrastructure.

The API no longer owns local implementations of:

```text
IEventPublisher
ServiceBusEventPublisher
ServiceBusClient registration
ServiceBusOptions
Service Bus processor lifecycle
```

Business events publish through:

```text
AI Discovery event
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

Audit events publish through:

```text
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
Azure Service Bus
```

The AI Discovery domain events inherit from the shared
`Algocura.Platform.Messaging.Events.IntegrationEvent` through the
service-specific `AIDiscoveryIntegrationEvent` base record. This keeps the
service-owned source value:

```text
Algocura.AIDiscovery.Api
```

The `PlatformEventsConsumer` now inherits from `ServiceBusConsumerBase`.
The shared base owns processor creation, startup, shutdown, disposal, common
Service Bus options, and processor error logging. The AI Discovery consumer
continues to own its domain-specific event routing and projection updates.

## Registration

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

No API-local registration of `IEventPublisher` or `ServiceBusEventPublisher`
is required because the shared extension registers them.

Audit events use:

```csharp
builder.Services
    .AddAlgocuraAuditing<
        ServiceBusAuditEventPublisher>(
        options =>
        {
            builder.Configuration
                .GetSection(
                    AuditingOptions.SectionName)
                .Bind(options);
        });
```

## Configuration

```json
"ServiceBus": {
  "UseEmulator": true,
  "ConnectionString": "...",
  "FullyQualifiedNamespace": "",
  "TopicName": "algocura-events",
  "SubscriptionName": "ai-discovery-api",
  "DefaultMessageTimeToLiveDays": 7,
  "MaxConcurrentCalls": 4,
  "PrefetchCount": 0,
  "MaxAutoLockRenewalMinutes": 5
},
"Auditing": {
  "ServiceName": "Algocura.AIDiscovery.Api",
  "PublishEntityAuditEvents": true,
  "StampCreatedAndUpdatedFields": true,
  "IncludeUnchangedPropertiesOnCreate": true,
  "IncludeUnchangedPropertiesOnDelete": true,
  "IgnoreAuditInfrastructureEntities": true
}
```

`SubscriptionName` is required because AI Discovery consumes platform events.


---

## Adaptive Molecular-Model Routing

The API now supports a prototype adaptive routing layer using the molecular and
workflow information currently available in AI Discovery: molecular weight,
screening context, prior model confidence, model specialization, and a resource
budget.

```text
POST /api/v1/predictions/{id}/adaptive-run
```

Example:

```json
{ "resourceBudget": 1.0 }
```

The router scores eligible active models using molecular suitability, expected
confidence, specialization, and estimated resource cost. If prototype
confidence is below `AdaptiveRouting:MinimumAcceptedConfidence`, the
orchestrator can route to another eligible model while budget remains.

The current confidence calculation is explicitly a deterministic prototype.
Replace it with model-native calibrated confidence/uncertainty when real ML
models are integrated. The existing `/run` endpoint remains unchanged.

### Seeded adaptive-routing models

The prototype seed now creates a small heterogeneous model catalog so adaptive
routing can demonstrate actual model selection and escalation:

| Model | Role | Expected confidence | Resource cost |
|---|---|---:|---:|
| `QSAR Fast` | Fast first-pass model | 0.64 | 0.10 |
| `Algocura Deterministic Prioritizer` | Baseline deterministic model | 0.70 | 0.16 |
| `GNN Molecular` | Graph-oriented specialist | 0.78 | 0.25 |
| `SMILES Transformer` | Sequence-oriented escalation model | 0.84 | 0.32 |
| `Ensemble Consensus` | High-confidence final escalation | 0.91 | 0.45 |

The model IDs are deterministic so the `AdaptiveRouting:Models` configuration
can reference them safely across repeated development database initialization.

These are prototype model definitions. They currently execute through the
existing deterministic scoring engine with different model parameters; their
names describe the intended future model specialization. When actual QSAR,
GNN, transformer, and ensemble runtimes are integrated, the routing contracts
can remain while the execution implementation is replaced.



## Project definition (authoritative build metadata)

Source: `Algocura.AIDiscovery.Api/Algocura.AIDiscovery.Api.csproj`
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
		<PackageReference
		Include="Microsoft.EntityFrameworkCore.Design"
		Version="10.0.10">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>
				runtime;
				build;
				native;
				contentfiles;
				analyzers;
				buildtransitive
			</IncludeAssets>
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
| `Algocura.AIDiscovery.Api/Algocura.AIDiscovery.Api.csproj` | 34 |
| `Algocura.AIDiscovery.Api/Contracts/AdaptivePredictionContracts.cs` | 15 |
| `Algocura.AIDiscovery.Api/Contracts/PredictionContracts.cs` | 54 |
| `Algocura.AIDiscovery.Api/Controllers/ModelsController.cs` | 43 |
| `Algocura.AIDiscovery.Api/Controllers/PredictionsController.cs` | 554 |
| `Algocura.AIDiscovery.Api/Data/AIDiscoveryDbContext.cs` | 146 |
| `Algocura.AIDiscovery.Api/Data/AIDiscoverySeeder.cs` | 999 |
| `Algocura.AIDiscovery.Api/Data/DatabaseInitializer.cs` | 89 |
| `Algocura.AIDiscovery.Api/Data/Migrations/20260110052242_InitialCreate.Designer.cs` | 351 |
| `Algocura.AIDiscovery.Api/Data/Migrations/20260110052242_InitialCreate.cs` | 217 |
| `Algocura.AIDiscovery.Api/Data/Migrations/20260806015051_AddAuditMetadata.Designer.cs` | 490 |
| `Algocura.AIDiscovery.Api/Data/Migrations/20260806015051_AddAuditMetadata.cs` | 522 |
| `Algocura.AIDiscovery.Api/Data/Migrations/AIDiscoveryDbContextModelSnapshot.cs` | 487 |
| `Algocura.AIDiscovery.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.AIDiscovery.Api/Domain/Entities.cs` | 268 |
| `Algocura.AIDiscovery.Api/Events/IntegrationEvents.cs` | 155 |
| `Algocura.AIDiscovery.Api/Events/PlatformIntegrationEvents.cs` | 31 |
| `Algocura.AIDiscovery.Api/Mapping/PredictionMappings.cs` | 56 |
| `Algocura.AIDiscovery.Api/Messaging/Consumers/PlatformEventsConsumer.cs` | 417 |
| `Algocura.AIDiscovery.Api/Program.cs` | 415 |
| `Algocura.AIDiscovery.Api/Properties/launchSettings.json` | 23 |
| `Algocura.AIDiscovery.Api/README.md` | 165 |
| `Algocura.AIDiscovery.Api/Routing/AdaptiveModelRouter.cs` | 65 |
| `Algocura.AIDiscovery.Api/Routing/AdaptivePredictionOrchestrator.cs` | 87 |
| `Algocura.AIDiscovery.Api/Routing/AdaptiveRoutingContracts.cs` | 21 |
| `Algocura.AIDiscovery.Api/Routing/IAdaptiveModelRouter.cs` | 6 |
| `Algocura.AIDiscovery.Api/Routing/ModelRoutingOptions.cs` | 24 |
| `Algocura.AIDiscovery.Api/Services/PredictionScoring.cs` | 67 |
| `Algocura.AIDiscovery.Api/appsettings.json` | 99 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.AIDiscovery.Api/Controllers/ModelsController.cs`

- L10: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L12: `[Route("api/v1/models")]`
- L13: `public sealed class ModelsController(AIDiscoveryDbContext db)`
- L16: `[HttpGet]`
- L30: `[HttpGet("{id:guid}")]`

### `Algocura.AIDiscovery.Api/Controllers/PredictionsController.cs`

- L15: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L17: `[Route("api/v1/predictions")]`
- L26: `[HttpGet]`
- L57: `[HttpGet("{id:guid}")]`
- L69: `[HttpPost]`
- L225: `[HttpPost("{id:guid}/start")]`
- L265: `[HttpPost("{id:guid}/run")]`
- L358: `[HttpPost("{id:guid}/adaptive-run")]`
- L454: `[HttpPost("{id:guid}/fail")]`
- L498: `[HttpDelete("{id:guid}")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.AIDiscovery.Api/Contracts/AdaptivePredictionContracts.cs`
Types: `AdaptiveRunPredictionRequest`, `AdaptiveModelExecutionResponse`, `AdaptiveRunPredictionResponse`

### `Algocura.AIDiscovery.Api/Contracts/PredictionContracts.cs`
Types: `CreatePredictionRequest`, `FailPredictionRequest`, `PredictionCandidateResponse`, `PredictionResultResponse`, `PredictionResponse`, `ModelDefinitionResponse`

### `Algocura.AIDiscovery.Api/Data/AIDiscoveryDbContext.cs`
Types: `AIDiscoveryDbContext`
- L11: `public DbSet<CompoundProjection> Compounds => Set<CompoundProjection>();`
- L12: `public DbSet<ScreeningProjection> Screenings => Set<ScreeningProjection>();`
- L13: `public DbSet<ScreeningProjectionResult> ScreeningResults =>`
- L15: `public DbSet<ModelDefinition> ModelDefinitions =>`
- L17: `public DbSet<PredictionJob> PredictionJobs => Set<PredictionJob>();`
- L18: `public DbSet<PredictionCandidate> PredictionCandidates =>`
- L20: `public DbSet<PredictionResult> PredictionResults =>`
- L25: `modelBuilder.Entity<CompoundProjection>(entity =>`
- L27: `entity.HasKey(x => x.CompoundId);`
- L28: `entity.Property(x => x.MolecularWeight).HasPrecision(18, 4);`
- L31: `modelBuilder.Entity<ScreeningProjection>(entity =>`
- L33: `entity.HasKey(x => x.ScreeningId);`
- L34: `entity.HasMany(x => x.Results)`
- L40: `modelBuilder.Entity<ScreeningProjectionResult>(entity =>`
- L42: `entity.HasKey(x => x.Id);`
- L43: `entity.Property(x => x.Score).HasPrecision(18, 6);`
- L44: `entity.HasIndex(x => new { x.ScreeningId, x.CompoundId })`
- L48: `modelBuilder.Entity<ModelDefinition>(entity =>`
- L50: `entity.HasKey(x => x.Id);`
- L51: `entity.HasIndex(x => new { x.Name, x.Version }).IsUnique();`
- L52: `entity.Property(x => x.ScreeningScoreWeight).HasPrecision(18, 6);`
- L53: `entity.Property(x => x.MolecularWeightWeight).HasPrecision(18, 6);`
- L54: `entity.Property(x => x.SelectionBonus).HasPrecision(18, 6);`
- L57: `modelBuilder.Entity<PredictionJob>(entity =>`
- L59: `entity.HasKey(x => x.Id);`
- L61: `entity.HasOne(x => x.ModelDefinition)`
- L66: `entity.HasMany(x => x.Candidates)`
- L71: `entity.HasMany(x => x.Results)`
- L77: `modelBuilder.Entity<PredictionCandidate>(entity =>`
- L79: `entity.HasKey(x => x.Id);`
- L80: `entity.Property(x => x.ScreeningScore).HasPrecision(18, 6);`
- L81: `entity.Property(x => x.MolecularWeight).HasPrecision(18, 4);`
- L82: `entity.HasIndex(x => new { x.PredictionJobId, x.CompoundId })`
- L86: `modelBuilder.Entity<PredictionResult>(entity =>`
- L88: `entity.HasKey(x => x.Id);`
- L89: `entity.Property(x => x.PredictionScore).HasPrecision(18, 6);`
- L90: `entity.HasIndex(x => new { x.PredictionJobId, x.CompoundId })`
- L102: `modelBuilder.Model`
- L114: `modelBuilder.Entity(`
- L117: `entity.Property<Guid?>(`
- L122: `entity.Property<Guid?>(`
- L127: `entity.Property<DateTimeOffset>(`
- L132: `entity.Property<DateTimeOffset>(`
- L137: `entity.HasIndex(`
- L141: `entity.HasIndex(`

### `Algocura.AIDiscovery.Api/Data/AIDiscoverySeeder.cs`
Types: `AIDiscoverySeeder`

### `Algocura.AIDiscovery.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.AIDiscovery.Api/Data/Migrations/20260110052242_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.CompoundProjection", b =>`
- L67: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ModelDefinition", b =>`
- L116: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionCandidate", b =>`
- L155: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L202: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionResult", b =>`
- L236: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjection", b =>`
- L263: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjectionResult", b =>`
- L293: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionCandidate", b =>`
- L304: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L315: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionResult", b =>`
- L326: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjectionResult", b =>`
- L337: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L344: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjection", b =>`

### `Algocura.AIDiscovery.Api/Data/Migrations/20260110052242_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.AIDiscovery.Api/Data/Migrations/20260806015051_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.CompoundProjection", b =>`
- L86: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ModelDefinition", b =>`
- L151: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionCandidate", b =>`
- L206: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L272: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionResult", b =>`
- L334: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjection", b =>`
- L380: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjectionResult", b =>`
- L432: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionCandidate", b =>`
- L443: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L454: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionResult", b =>`
- L465: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjectionResult", b =>`
- L476: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L483: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjection", b =>`

### `Algocura.AIDiscovery.Api/Data/Migrations/20260806015051_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.AIDiscovery.Api/Data/Migrations/AIDiscoveryDbContextModelSnapshot.cs`
Types: `AIDiscoveryDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.CompoundProjection", b =>`
- L83: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ModelDefinition", b =>`
- L148: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionCandidate", b =>`
- L203: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L269: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionResult", b =>`
- L331: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjection", b =>`
- L377: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjectionResult", b =>`
- L429: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionCandidate", b =>`
- L440: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L451: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionResult", b =>`
- L462: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjectionResult", b =>`
- L473: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.PredictionJob", b =>`
- L480: `modelBuilder.Entity("Algocura.AIDiscovery.Api.Domain.ScreeningProjection", b =>`

### `Algocura.AIDiscovery.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.AIDiscovery.Api/Messaging/Consumers/PlatformEventsConsumer.cs`

### `Algocura.AIDiscovery.Api/Program.cs`
- L23 registration: `builder.Services.AddHealthChecks();`
- L36 registration: `builder.Services.AddCors(`
- L104 registration: `builder.Services.AddDbContext<AIDiscoveryDbContext>(`
- L141 registration: `builder.Services.AddScoped<`
- L152 registration: `builder.Services.AddScoped<IAdaptiveModelRouter, AdaptiveModelRouter>();`
- L153 registration: `builder.Services.AddScoped<IAdaptivePredictionOrchestrator, AdaptivePredictionOrchestrator>();`
- L237 registration: `app.UseAuthentication();`
- L238 registration: `app.UseAuthorization();`
- L303 registration: `app.MapControllers();`

### `Algocura.AIDiscovery.Api/Services/PredictionScoring.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
