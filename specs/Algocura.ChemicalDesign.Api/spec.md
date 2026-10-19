# Algocura.ChemicalDesign.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 24.

## Original documented intent

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

Source: `Algocura.ChemicalDesign.Api/Algocura.ChemicalDesign.Api.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk.Web">
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
		<Folder Include="Data\Migrations\" />
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
| `Algocura.ChemicalDesign.Api/Algocura.ChemicalDesign.Api.csproj` | 28 |
| `Algocura.ChemicalDesign.Api/Contracts/Contracts.cs` | 88 |
| `Algocura.ChemicalDesign.Api/Controllers/ChemicalDesignSynchronizationController.cs` | 164 |
| `Algocura.ChemicalDesign.Api/Controllers/SearchesAndCandidatesControllers.cs` | 364 |
| `Algocura.ChemicalDesign.Api/Controllers/TargetsAndProjectsControllers.cs` | 192 |
| `Algocura.ChemicalDesign.Api/Data/ChemicalDesignDbContext.cs` | 313 |
| `Algocura.ChemicalDesign.Api/Data/ChemicalDesignDbContextFactory.cs` | 19 |
| `Algocura.ChemicalDesign.Api/Data/ChemicalDesignSeeder.cs` | 882 |
| `Algocura.ChemicalDesign.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.ChemicalDesign.Api/Data/Migrations/20260113030056_InitialCreate.Designer.cs` | 530 |
| `Algocura.ChemicalDesign.Api/Data/Migrations/20260113030056_InitialCreate.cs` | 323 |
| `Algocura.ChemicalDesign.Api/Data/Migrations/20260805061559_AddAuditMetadata.Designer.cs` | 655 |
| `Algocura.ChemicalDesign.Api/Data/Migrations/20260805061559_AddAuditMetadata.cs` | 479 |
| `Algocura.ChemicalDesign.Api/Data/Migrations/ChemicalDesignDbContextModelSnapshot.cs` | 652 |
| `Algocura.ChemicalDesign.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.ChemicalDesign.Api/Domain/Entities.cs` | 198 |
| `Algocura.ChemicalDesign.Api/Events/IntegrationEvents.cs` | 66 |
| `Algocura.ChemicalDesign.Api/Mapping/Mappings.cs` | 48 |
| `Algocura.ChemicalDesign.Api/Messaging/Consumers/TargetDiscoveryEventsConsumer.cs` | 220 |
| `Algocura.ChemicalDesign.Api/Program.cs` | 403 |
| `Algocura.ChemicalDesign.Api/Properties/launchSettings.json` | 23 |
| `Algocura.ChemicalDesign.Api/README.md` | 109 |
| `Algocura.ChemicalDesign.Api/Services/StructureSearchEngine.cs` | 70 |
| `Algocura.ChemicalDesign.Api/appsettings.json` | 45 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.ChemicalDesign.Api/Controllers/ChemicalDesignSynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/chemical-design/synchronization")]`
- L20: `[HttpPost]`
- L81: `[HttpPost("{id:guid}")]`

### `Algocura.ChemicalDesign.Api/Controllers/SearchesAndCandidatesControllers.cs`

- L14: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L16: `[Route("api/v1/structure-searches")]`
- L22: `[HttpGet("{id:guid}")]`
- L33: `[HttpPost("/api/v1/design-projects/{projectId:guid}/structure-searches")]`
- L94: `[HttpPost("{id:guid}/run")]`
- L256: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L258: `[Route("api/v1/candidates")]`
- L263: `[HttpGet("{id:guid}")]`
- L273: `[HttpPost("{id:guid}/select-for-registration")]`
- L311: `[HttpPost("{id:guid}/synthesis-routes")]`

### `Algocura.ChemicalDesign.Api/Controllers/TargetsAndProjectsControllers.cs`

- L13: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L15: `[Route("api/v1/selected-targets")]`
- L16: `public sealed class SelectedTargetsController(ChemicalDesignDbContext db) : ControllerBase`
- L18: `[HttpGet]`
- L43: `[HttpGet("{id:guid}")]`
- L54: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L56: `[Route("api/v1/design-projects")]`
- L62: `[HttpGet("{id:guid}")]`
- L70: `[HttpPost]`
- L121: `[HttpPost("{id:guid}/query-structures")]`
- L161: `[HttpPost("{id:guid}/complete")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.ChemicalDesign.Api/Contracts/Contracts.cs`
Types: `CreateDesignProjectRequest`, `CreateQueryStructureRequest`, `CreateStructureSearchRequest`, `SelectCandidateRequest`, `CreateSynthesisRouteRequest`, `CreateReactionStepRequest`, `SelectedTargetResponse`, `QueryStructureResponse`, `CandidateStructureResponse`, `StructureSearchResponse`, `ReactionStepResponse`, `SynthesisRouteResponse`, `DesignProjectResponse`

### `Algocura.ChemicalDesign.Api/Data/ChemicalDesignDbContext.cs`
Types: `ChemicalDesignDbContext`
- L11: `public DbSet<SelectedTargetProjection> SelectedTargets =>`
- L14: `public DbSet<ChemicalDesignProject> DesignProjects =>`
- L17: `public DbSet<QueryStructure> QueryStructures =>`
- L20: `public DbSet<StructureSearchJob> StructureSearchJobs =>`
- L23: `public DbSet<CandidateStructure> CandidateStructures =>`
- L26: `public DbSet<SynthesisRoute> SynthesisRoutes =>`
- L29: `public DbSet<ReactionStep> ReactionSteps =>`
- L32: `public DbSet<ChemicalLibraryEntry> ChemicalLibrary =>`
- L38: `modelBuilder.Entity<SelectedTargetProjection>(`
- L41: `entity.HasKey(`
- L44: `entity.Property(`
- L48: `entity.Property(`
- L52: `entity.Property(`
- L56: `entity.HasIndex(`
- L59: `entity.HasIndex(`
- L62: `entity.HasMany(`
- L72: `modelBuilder.Entity<ChemicalDesignProject>(`
- L75: `entity.HasKey(`
- L78: `entity.Property(`
- L82: `entity.HasIndex(`
- L85: `entity.HasIndex(`
- L88: `entity.HasMany(`
- L97: `entity.HasMany(`
- L106: `entity.HasMany(`
- L116: `modelBuilder.Entity<QueryStructure>(`
- L119: `entity.HasKey(`
- L122: `entity.Property(`
- L127: `modelBuilder.Entity<StructureSearchJob>(`
- L130: `entity.HasKey(`
- L133: `entity.Property(`
- L137: `entity.Property(`
- L141: `entity.HasOne(`
- L149: `entity.HasMany(`
- L159: `modelBuilder.Entity<CandidateStructure>(`
- L162: `entity.HasKey(`
- L165: `entity.Property(`
- L169: `entity.Property(`
- L173: `entity.Property(`
- L177: `entity.HasIndex(`
- L180: `entity.HasIndex(`
- L183: `entity.HasMany(`
- L193: `modelBuilder.Entity<SynthesisRoute>(`
- L196: `entity.HasKey(`
- L199: `entity.Property(`
- L203: `entity.Property(`
- L207: `entity.HasMany(`
- L217: `modelBuilder.Entity<ReactionStep>(`
- L220: `entity.HasKey(`
- L223: `entity.Property(`
- L227: `entity.Property(`
- L231: `entity.HasIndex(`
- L240: `modelBuilder.Entity<ChemicalLibraryEntry>(`
- L243: `entity.HasKey(`
- L246: `entity.Property(`
- L250: `entity.Property(`
- L254: `entity.HasIndex(`
- L269: `modelBuilder.Model`
- L281: `modelBuilder.Entity(`
- L284: `entity.Property<Guid?>(`
- L289: `entity.Property<Guid?>(`
- L294: `entity.Property<DateTimeOffset>(`
- L299: `entity.Property<DateTimeOffset>(`
- L304: `entity.HasIndex(`
- L308: `entity.HasIndex(`

### `Algocura.ChemicalDesign.Api/Data/ChemicalDesignDbContextFactory.cs`
Types: `ChemicalDesignDbContextFactory`

### `Algocura.ChemicalDesign.Api/Data/ChemicalDesignSeeder.cs`
Types: `ChemicalDesignSeeder`

### `Algocura.ChemicalDesign.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.ChemicalDesign.Api/Data/Migrations/20260113030056_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L98: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L141: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalLibraryEntry", b =>`
- L182: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.QueryStructure", b =>`
- L219: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ReactionStep", b =>`
- L263: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SelectedTargetProjection", b =>`
- L323: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L377: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`
- L417: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L436: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L447: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.QueryStructure", b =>`
- L458: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ReactionStep", b =>`
- L469: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L488: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`
- L499: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L504: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L513: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SelectedTargetProjection", b =>`
- L518: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L523: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`

### `Algocura.ChemicalDesign.Api/Data/Migrations/20260113030056_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.ChemicalDesign.Api/Data/Migrations/20260805061559_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L111: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L170: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalLibraryEntry", b =>`
- L227: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.QueryStructure", b =>`
- L277: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ReactionStep", b =>`
- L337: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SelectedTargetProjection", b =>`
- L419: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L486: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`
- L542: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L561: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L572: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.QueryStructure", b =>`
- L583: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ReactionStep", b =>`
- L594: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L613: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`
- L624: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L629: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L638: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SelectedTargetProjection", b =>`
- L643: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L648: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`

### `Algocura.ChemicalDesign.Api/Data/Migrations/20260805061559_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.ChemicalDesign.Api/Data/Migrations/ChemicalDesignDbContextModelSnapshot.cs`
Types: `ChemicalDesignDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L108: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L167: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalLibraryEntry", b =>`
- L224: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.QueryStructure", b =>`
- L274: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ReactionStep", b =>`
- L334: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SelectedTargetProjection", b =>`
- L416: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L483: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`
- L539: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L558: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L569: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.QueryStructure", b =>`
- L580: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ReactionStep", b =>`
- L591: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L610: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`
- L621: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.CandidateStructure", b =>`
- L626: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.ChemicalDesignProject", b =>`
- L635: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SelectedTargetProjection", b =>`
- L640: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.StructureSearchJob", b =>`
- L645: `modelBuilder.Entity("Algocura.ChemicalDesign.Api.Domain.SynthesisRoute", b =>`

### `Algocura.ChemicalDesign.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.ChemicalDesign.Api/Messaging/Consumers/TargetDiscoveryEventsConsumer.cs`

### `Algocura.ChemicalDesign.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<ChemicalDesignDbContext>(`
- L140 registration: `builder.Services.AddScoped<`
- L225 registration: `app.UseAuthentication();`
- L226 registration: `app.UseAuthorization();`
- L291 registration: `app.MapControllers();`

### `Algocura.ChemicalDesign.Api/Services/StructureSearchEngine.cs`
- L40: `private static decimal Score(string q, string c, StructureSearchType type)`
- L58: `private static HashSet<string> Tokens(string value)`
- L67: `private static string Normalize(string value) =>`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
