# Algocura.LiteratureIntelligence.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 23.

## Original documented intent

# Algocura Literature Intelligence API

The service starts the Algocura discovery workflow:

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

It registers publications, extracts reviewable disease-gene evidence,
supports human validation, creates target hypotheses, and publishes
integration events for downstream services.

## Prototype limitation

`RuleBasedLiteratureAnalysisService` is deterministic placeholder logic.
It is not a biomedical language model and does not scientifically validate
a disease-gene relationship. Human review remains required.

## Local URLs

- `http://localhost:7004/`
- `http://localhost:7004/swagger`
- `http://localhost:7004/redoc`
- `http://localhost:7004/openapi/v1.json`
- `http://localhost:7004/health`

## Routes

- `GET /api/v1/publications`
- `GET /api/v1/publications/{id}`
- `POST /api/v1/publications`
- `POST /api/v1/publications/{id}/analyze`
- `DELETE /api/v1/publications/{id}`
- `GET /api/v1/evidence`
- `POST /api/v1/evidence/{id}/validate`
- `GET /api/v1/target-hypotheses`
- `GET /api/v1/target-hypotheses/{id}`
- `POST /api/v1/target-hypotheses`
- `POST /api/v1/target-hypotheses/{id}/publish`
- `POST /api/v1/literature-intelligence/synchronization`

## Events

- `PublicationRegistered`
- `LiteratureEvidenceDiscovered`
- `TargetHypothesisProposed`
- `LiteratureIntelligenceSnapshot`

## Migrations

The starter uses `EnsureCreatedAsync()` so it runs immediately.
After validating the model:

```powershell
dotnet ef migrations add InitialCreate `
  --project .\Algocura.LiteratureIntelligence.Api\Algocura.LiteratureIntelligence.Api.csproj `
  --startup-project .\Algocura.LiteratureIntelligence.Api\Algocura.LiteratureIntelligence.Api.csproj `
  --context LiteratureIntelligenceDbContext `
  --output-dir Migrations
```

Then replace `EnsureCreatedAsync()` with `MigrateAsync()`.

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

This service is publisher-only and does not require a Service Bus subscription.

Do not add API-local copies of `IEventPublisher`, `ServiceBusEventPublisher`,
or `ServiceBusOptions`.



## Project definition (authoritative build metadata)

Source: `Algocura.LiteratureIntelligence.Api/Algocura.LiteratureIntelligence.Api.csproj`
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
| `Algocura.LiteratureIntelligence.Api/Algocura.LiteratureIntelligence.Api.csproj` | 32 |
| `Algocura.LiteratureIntelligence.Api/Contracts/Contracts.cs` | 66 |
| `Algocura.LiteratureIntelligence.Api/Controllers/EvidenceController.cs` | 85 |
| `Algocura.LiteratureIntelligence.Api/Controllers/LiteratureSynchronizationController.cs` | 112 |
| `Algocura.LiteratureIntelligence.Api/Controllers/PublicationsController.cs` | 249 |
| `Algocura.LiteratureIntelligence.Api/Controllers/TargetHypothesesController.cs` | 199 |
| `Algocura.LiteratureIntelligence.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.LiteratureIntelligence.Api/Data/LiteratureIntelligenceDbContext.cs` | 168 |
| `Algocura.LiteratureIntelligence.Api/Data/LiteratureIntelligenceSeeder.cs` | 489 |
| `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260111051310_InitialCreate.Designer.cs` | 276 |
| `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260111051310_InitialCreate.cs` | 178 |
| `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260805044004_AddAuditMetadata.Designer.cs` | 306 |
| `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260805044004_AddAuditMetadata.cs` | 118 |
| `Algocura.LiteratureIntelligence.Api/Data/Migrations/LiteratureIntelligenceDbContextModelSnapshot.cs` | 303 |
| `Algocura.LiteratureIntelligence.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.LiteratureIntelligence.Api/Domain/Entities.cs` | 170 |
| `Algocura.LiteratureIntelligence.Api/Events/IntegrationEvents.cs` | 71 |
| `Algocura.LiteratureIntelligence.Api/Mapping/Mappings.cs` | 68 |
| `Algocura.LiteratureIntelligence.Api/Program.cs` | 395 |
| `Algocura.LiteratureIntelligence.Api/Properties/launchSettings.json` | 23 |
| `Algocura.LiteratureIntelligence.Api/README.md` | 109 |
| `Algocura.LiteratureIntelligence.Api/Services/LiteratureAnalysisService.cs` | 132 |
| `Algocura.LiteratureIntelligence.Api/appsettings.json` | 43 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.LiteratureIntelligence.Api/Controllers/EvidenceController.cs`

- L10: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L12: `[Route("api/v1/evidence")]`
- L17: `[HttpGet]`
- L65: `[HttpPost("{id:guid}/validate")]`

### `Algocura.LiteratureIntelligence.Api/Controllers/LiteratureSynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/literature-intelligence/synchronization")]`
- L20: `[HttpPost]`

### `Algocura.LiteratureIntelligence.Api/Controllers/PublicationsController.cs`

- L14: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L16: `[Route("api/v1/publications")]`
- L24: `[HttpGet]`
- L66: `[HttpGet("{id:guid}")]`
- L82: `[HttpPost]`
- L160: `[HttpPost("{id:guid}/analyze")]`
- L228: `[HttpDelete("{id:guid}")]`

### `Algocura.LiteratureIntelligence.Api/Controllers/TargetHypothesesController.cs`

- L13: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L15: `[Route("api/v1/target-hypotheses")]`
- L22: `[HttpGet]`
- L55: `[HttpGet("{id:guid}")]`
- L72: `[HttpPost]`
- L159: `[HttpPost("{id:guid}/publish")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.LiteratureIntelligence.Api/Contracts/Contracts.cs`
Types: `CreatePublicationRequest`, `AnalyzePublicationRequest`, `CreateTargetHypothesisRequest`, `EvidenceStatementResponse`, `PublicationResponse`, `TargetHypothesisResponse`

### `Algocura.LiteratureIntelligence.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.LiteratureIntelligence.Api/Data/LiteratureIntelligenceDbContext.cs`
Types: `LiteratureIntelligenceDbContext`
- L11: `public DbSet<Publication> Publications =>`
- L14: `public DbSet<PublicationAuthor> PublicationAuthors =>`
- L17: `public DbSet<EvidenceStatement> EvidenceStatements =>`
- L20: `public DbSet<TargetHypothesis> TargetHypotheses =>`
- L23: `public DbSet<TargetHypothesisEvidence>`
- L30: `modelBuilder.Entity<Publication>(entity =>`
- L32: `entity.HasKey(x => x.Id);`
- L34: `entity.Property(x => x.Id)`
- L37: `entity.HasIndex(x => x.ExternalId)`
- L41: `entity.HasMany(x => x.Authors)`
- L47: `entity.HasMany(x => x.EvidenceStatements)`
- L54: `modelBuilder.Entity<PublicationAuthor>(entity =>`
- L56: `entity.HasKey(x => x.Id);`
- L58: `entity.Property(x => x.Id)`
- L61: `entity.HasIndex(x => new`
- L69: `modelBuilder.Entity<EvidenceStatement>(entity =>`
- L71: `entity.HasKey(x => x.Id);`
- L73: `entity.Property(x => x.Id)`
- L76: `entity.Property(x => x.ConfidenceScore)`
- L79: `entity.HasIndex(x => x.DiseaseName);`
- L81: `entity.HasIndex(x => x.GeneSymbol);`
- L84: `modelBuilder.Entity<TargetHypothesis>(entity =>`
- L86: `entity.HasKey(x => x.Id);`
- L88: `entity.Property(x => x.Id)`
- L91: `entity.Property(x => x.EvidenceScore)`
- L95: `modelBuilder.Entity<TargetHypothesisEvidence>(entity =>`
- L97: `entity.HasKey(x => new`
- L103: `entity.HasOne(x => x.TargetHypothesis)`
- L109: `entity.HasOne(x => x.EvidenceStatement)`
- L124: `modelBuilder.Model`
- L136: `modelBuilder.Entity(`
- L139: `entity.Property<Guid?>(`
- L144: `entity.Property<Guid?>(`
- L149: `entity.Property<DateTimeOffset>(`
- L154: `entity.Property<DateTimeOffset>(`
- L159: `entity.HasIndex(`
- L163: `entity.HasIndex(`

### `Algocura.LiteratureIntelligence.Api/Data/LiteratureIntelligenceSeeder.cs`
Types: `LiteratureIntelligenceSeeder`

### `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260111051310_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.EvidenceStatement", b =>`
- L84: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.Publication", b =>`
- L135: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.PublicationAuthor", b =>`
- L159: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesis", b =>`
- L206: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesisEvidence", b =>`
- L221: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.EvidenceStatement", b =>`
- L232: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.PublicationAuthor", b =>`
- L243: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesisEvidence", b =>`
- L262: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.Publication", b =>`
- L269: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesis", b =>`

### `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260111051310_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260805044004_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.EvidenceStatement", b =>`
- L90: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.Publication", b =>`
- L147: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.PublicationAuthor", b =>`
- L177: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesis", b =>`
- L230: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesisEvidence", b =>`
- L251: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.EvidenceStatement", b =>`
- L262: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.PublicationAuthor", b =>`
- L273: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesisEvidence", b =>`
- L292: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.Publication", b =>`
- L299: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesis", b =>`

### `Algocura.LiteratureIntelligence.Api/Data/Migrations/20260805044004_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.LiteratureIntelligence.Api/Data/Migrations/LiteratureIntelligenceDbContextModelSnapshot.cs`
Types: `LiteratureIntelligenceDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.EvidenceStatement", b =>`
- L87: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.Publication", b =>`
- L144: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.PublicationAuthor", b =>`
- L174: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesis", b =>`
- L227: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesisEvidence", b =>`
- L248: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.EvidenceStatement", b =>`
- L259: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.PublicationAuthor", b =>`
- L270: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesisEvidence", b =>`
- L289: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.Publication", b =>`
- L296: `modelBuilder.Entity("Algocura.LiteratureIntelligence.Api.Domain.TargetHypothesis", b =>`

### `Algocura.LiteratureIntelligence.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.LiteratureIntelligence.Api/Program.cs`
- L21 registration: `builder.Services.AddHealthChecks();`
- L34 registration: `builder.Services.AddCors(`
- L102 registration: `builder.Services.AddDbContext<LiteratureIntelligenceDbContext>(`
- L139 registration: `builder.Services.AddScoped<`
- L220 registration: `app.UseAuthentication();`
- L221 registration: `app.UseAuthorization();`
- L286 registration: `app.MapControllers();`

### `Algocura.LiteratureIntelligence.Api/Services/LiteratureAnalysisService.cs`
- L98: `private static string InferRelationshipType(string text)`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
