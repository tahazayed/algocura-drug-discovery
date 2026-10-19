# Algocura.TargetDiscovery.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 26.

## Original documented intent

# Algocura Target Discovery API

The **Algocura Target Discovery API** is the second service in the
Algocura discovery workflow:

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

The service consumes literature-derived target hypotheses, maintains
local projections, manages target assessments, calculates prototype
assessment scores, selects targets, and publishes selected-target
snapshots for the Chemical Design API.

## Prototype limitation

The included scoring engine is deterministic and intended to validate
the workflow and service architecture. It does not scientifically
establish target validity, safety, or druggability. Human scientific
review and validated experimental evidence remain required.

## Features

- Literature target-hypothesis synchronization
- Disease and gene target projections
- Genetic evidence scoring
- Biological plausibility scoring
- Druggability scoring
- Safety-risk scoring
- Translational-potential scoring
- Deterministic overall scoring
- Target selection
- Selected-target synchronization
- SQL Server and Entity Framework Core
- Azure Service Bus consumer and publisher
- Swagger, ReDoc, and health endpoints
- Docker support
- xUnit, FluentAssertions, NSubstitute, and SQLite tests

## Local endpoints

| Resource | URL |
|---|---|
| Service information | `http://localhost:7002/` |
| Swagger | `http://localhost:7002/swagger` |
| ReDoc | `http://localhost:7002/redoc` |
| OpenAPI | `http://localhost:7002/openapi/v1.json` |
| Health | `http://localhost:7002/health` |

## API routes

### Target hypotheses

- `GET /api/v1/target-hypotheses`
- `GET /api/v1/target-hypotheses/{id}`

### Target assessments

- `GET /api/v1/target-assessments`
- `GET /api/v1/target-assessments/{id}`
- `POST /api/v1/target-assessments`
- `PUT /api/v1/target-assessments/{id}`
- `POST /api/v1/target-assessments/{id}/complete`
- `POST /api/v1/target-assessments/{id}/select`
- `DELETE /api/v1/target-assessments/{id}`

### Synchronization

- `POST /api/v1/target-discovery/synchronization`
- `POST /api/v1/target-discovery/synchronization/{id}`

## Event consumed

- `LiteratureIntelligenceSnapshot`

## Events published

- `TargetAssessmentCreated`
- `TargetAssessmentCompleted`
- `TargetSelected`
- `TargetDiscoverySnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()` so it can run immediately.

After validating the model, create the initial migration:

```powershell
dotnet ef migrations add InitialCreate `
  --project .\Algocura.TargetDiscovery.Api\Algocura.TargetDiscovery.Api.csproj `
  --startup-project .\Algocura.TargetDiscovery.Api\Algocura.TargetDiscovery.Api.csproj `
  --context TargetDiscoveryDbContext `
  --output-dir Migrations
```

Then replace `EnsureCreatedAsync()` with:

```csharp
await dbContext.Database.MigrateAsync(cancellationToken);
```

## Build and test

```bash
dotnet restore
dotnet build
dotnet test
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

Source: `Algocura.TargetDiscovery.Api/Algocura.TargetDiscovery.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<RootNamespace>Algocura.TargetDiscovery.Api</RootNamespace>
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
| `Algocura.TargetDiscovery.Api/Algocura.TargetDiscovery.Api.csproj` | 31 |
| `Algocura.TargetDiscovery.Api/Contracts/TargetContracts.cs` | 57 |
| `Algocura.TargetDiscovery.Api/Controllers/TargetAssessmentsController.cs` | 413 |
| `Algocura.TargetDiscovery.Api/Controllers/TargetDiscoverySynchronizationController.cs` | 197 |
| `Algocura.TargetDiscovery.Api/Controllers/TargetHypothesesController.cs` | 79 |
| `Algocura.TargetDiscovery.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.TargetDiscovery.Api/Data/Migrations/20260111054147_InitialCreate.Designer.cs` | 199 |
| `Algocura.TargetDiscovery.Api/Data/Migrations/20260111054147_InitialCreate.cs` | 119 |
| `Algocura.TargetDiscovery.Api/Data/Migrations/20260805055502_AddAuditMetadata.Designer.cs` | 237 |
| `Algocura.TargetDiscovery.Api/Data/Migrations/20260805055502_AddAuditMetadata.cs` | 158 |
| `Algocura.TargetDiscovery.Api/Data/Migrations/TargetDiscoveryDbContextModelSnapshot.cs` | 234 |
| `Algocura.TargetDiscovery.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.TargetDiscovery.Api/Data/TargetDiscoveryDbContext.cs` | 129 |
| `Algocura.TargetDiscovery.Api/Data/TargetDiscoveryDbContextFactory.cs` | 23 |
| `Algocura.TargetDiscovery.Api/Data/TargetDiscoverySeeder.cs` | 265 |
| `Algocura.TargetDiscovery.Api/Domain/Enums.cs` | 27 |
| `Algocura.TargetDiscovery.Api/Domain/TargetAssessment.cs` | 51 |
| `Algocura.TargetDiscovery.Api/Domain/TargetHypothesisProjection.cs` | 63 |
| `Algocura.TargetDiscovery.Api/Events/IntegrationEvents.cs` | 97 |
| `Algocura.TargetDiscovery.Api/Mapping/TargetMappings.cs` | 50 |
| `Algocura.TargetDiscovery.Api/Messaging/Consumers/LiteratureIntelligenceEventsConsumer.cs` | 246 |
| `Algocura.TargetDiscovery.Api/Program.cs` | 403 |
| `Algocura.TargetDiscovery.Api/Properties/launchSettings.json` | 23 |
| `Algocura.TargetDiscovery.Api/README.md` | 148 |
| `Algocura.TargetDiscovery.Api/Services/TargetAssessmentScoringEngine.cs` | 85 |
| `Algocura.TargetDiscovery.Api/appsettings.json` | 43 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.TargetDiscovery.Api/Controllers/TargetAssessmentsController.cs`

- L14: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L16: `[Route("api/v1/target-assessments")]`
- L24: `[HttpGet]`
- L57: `[HttpGet("{id:guid}")]`
- L73: `[HttpPost]`
- L169: `[HttpPut("{id:guid}")]`
- L247: `[HttpPost("{id:guid}/complete")]`
- L310: `[HttpPost("{id:guid}/select")]`
- L371: `[HttpDelete("{id:guid}")]`

### `Algocura.TargetDiscovery.Api/Controllers/TargetDiscoverySynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/target-discovery/synchronization")]`
- L20: `[HttpPost]`
- L112: `[HttpPost("{id:guid}")]`

### `Algocura.TargetDiscovery.Api/Controllers/TargetHypothesesController.cs`

- L10: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L12: `[Route("api/v1/target-hypotheses")]`
- L17: `[HttpGet]`
- L63: `[HttpGet("{id:guid}")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.TargetDiscovery.Api/Contracts/TargetContracts.cs`
Types: `CreateTargetAssessmentRequest`, `UpdateTargetAssessmentRequest`, `TargetHypothesisResponse`, `TargetAssessmentResponse`

### `Algocura.TargetDiscovery.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.TargetDiscovery.Api/Data/Migrations/20260111054147_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetAssessment", b =>`
- L104: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisEvidenceProjection", b =>`
- L117: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisProjection", b =>`
- L168: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetAssessment", b =>`
- L179: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisEvidenceProjection", b =>`
- L190: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisProjection", b =>`

### `Algocura.TargetDiscovery.Api/Data/Migrations/20260111054147_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.TargetDiscovery.Api/Data/Migrations/20260805055502_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetAssessment", b =>`
- L110: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisEvidenceProjection", b =>`
- L139: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisProjection", b =>`
- L206: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetAssessment", b =>`
- L217: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisEvidenceProjection", b =>`
- L228: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisProjection", b =>`

### `Algocura.TargetDiscovery.Api/Data/Migrations/20260805055502_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.TargetDiscovery.Api/Data/Migrations/TargetDiscoveryDbContextModelSnapshot.cs`
Types: `TargetDiscoveryDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetAssessment", b =>`
- L107: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisEvidenceProjection", b =>`
- L136: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisProjection", b =>`
- L203: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetAssessment", b =>`
- L214: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisEvidenceProjection", b =>`
- L225: `modelBuilder.Entity("Algocura.TargetDiscovery.Api.Domain.TargetHypothesisProjection", b =>`

### `Algocura.TargetDiscovery.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

### `Algocura.TargetDiscovery.Api/Data/TargetDiscoveryDbContext.cs`
Types: `TargetDiscoveryDbContext`
- L11: `public DbSet<TargetHypothesisProjection> TargetHypotheses =>`
- L14: `public DbSet<TargetHypothesisEvidenceProjection> TargetHypothesisEvidence =>`
- L17: `public DbSet<TargetAssessment> TargetAssessments =>`
- L22: `modelBuilder.Entity<TargetHypothesisProjection>(entity =>`
- L24: `entity.HasKey(x => x.TargetHypothesisId);`
- L26: `entity.Property(x => x.TargetHypothesisId)`
- L29: `entity.Property(x => x.LiteratureEvidenceScore)`
- L32: `entity.HasIndex(x => x.DiseaseName);`
- L33: `entity.HasIndex(x => x.GeneSymbol);`
- L35: `entity.HasMany(x => x.EvidenceLinks)`
- L40: `entity.HasMany(x => x.Assessments)`
- L46: `modelBuilder.Entity<TargetHypothesisEvidenceProjection>(entity =>`
- L48: `entity.HasKey(x => new`
- L55: `modelBuilder.Entity<TargetAssessment>(entity =>`
- L57: `entity.HasKey(x => x.Id);`
- L59: `entity.Property(x => x.Id)`
- L62: `entity.Property(x => x.LiteratureEvidenceScore)`
- L65: `entity.Property(x => x.GeneticEvidenceScore)`
- L68: `entity.Property(x => x.BiologicalPlausibilityScore)`
- L71: `entity.Property(x => x.DruggabilityScore)`
- L74: `entity.Property(x => x.SafetyRiskScore)`
- L77: `entity.Property(x => x.TranslationalPotentialScore)`
- L80: `entity.Property(x => x.OverallScore)`
- L83: `entity.HasIndex(x => x.TargetHypothesisId);`
- L84: `entity.HasIndex(x => x.Status);`
- L94: `modelBuilder.Model`
- L104: `modelBuilder.Entity(entityType.ClrType);`
- L106: `entity.Property<Guid?>(`
- L110: `entity.Property<Guid?>(`
- L114: `entity.Property<DateTimeOffset>(`
- L118: `entity.Property<DateTimeOffset>(`
- L122: `entity.HasIndex(`
- L125: `entity.HasIndex(`

### `Algocura.TargetDiscovery.Api/Data/TargetDiscoveryDbContextFactory.cs`
Types: `TargetDiscoveryDbContextFactory`

### `Algocura.TargetDiscovery.Api/Data/TargetDiscoverySeeder.cs`
Types: `TargetDiscoverySeeder`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.TargetDiscovery.Api/Messaging/Consumers/LiteratureIntelligenceEventsConsumer.cs`

### `Algocura.TargetDiscovery.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<TargetDiscoveryDbContext>(`
- L140 registration: `builder.Services.AddScoped<`
- L225 registration: `app.UseAuthentication();`
- L226 registration: `app.UseAuthorization();`
- L291 registration: `app.MapControllers();`

### `Algocura.TargetDiscovery.Api/Services/TargetAssessmentScoringEngine.cs`
- L17: `public TargetAssessmentScore Calculate(TargetAssessment assessment)`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
