# Algocura.Search.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 25.

## Original documented intent

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

Source: `Algocura.Search.Api/Algocura.Search.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<RootNamespace>Algocura.Search.Api</RootNamespace>
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
| `Algocura.Search.Api/Algocura.Search.Api.csproj` | 27 |
| `Algocura.Search.Api/Contracts/SearchContracts.cs` | 51 |
| `Algocura.Search.Api/Controllers/SearchController.cs` | 47 |
| `Algocura.Search.Api/Controllers/SearchDocumentsController.cs` | 86 |
| `Algocura.Search.Api/Controllers/SearchSynchronizationController.cs` | 92 |
| `Algocura.Search.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.Search.Api/Data/Migrations/20260114025830_InitialCreate.Designer.cs` | 176 |
| `Algocura.Search.Api/Data/Migrations/20260114025830_InitialCreate.cs` | 122 |
| `Algocura.Search.Api/Data/Migrations/20260806012829_AddAuditMetadata.Designer.cs` | 211 |
| `Algocura.Search.Api/Data/Migrations/20260806012829_AddAuditMetadata.cs` | 147 |
| `Algocura.Search.Api/Data/Migrations/SearchDbContextModelSnapshot.cs` | 208 |
| `Algocura.Search.Api/Data/SearchDbContext.cs` | 100 |
| `Algocura.Search.Api/Data/SearchDbContextFactory.cs` | 25 |
| `Algocura.Search.Api/Data/SearchSeeder.cs` | 545 |
| `Algocura.Search.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.Search.Api/Domain/SearchDocument.cs` | 83 |
| `Algocura.Search.Api/Events/IntegrationEvents.cs` | 37 |
| `Algocura.Search.Api/Mapping/SearchMappings.cs` | 42 |
| `Algocura.Search.Api/Messaging/Consumers/PlatformSearchEventsConsumer.cs` | 251 |
| `Algocura.Search.Api/Program.cs` | 402 |
| `Algocura.Search.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Search.Api/README.md` | 93 |
| `Algocura.Search.Api/Services/SearchDocumentFactory.cs` | 238 |
| `Algocura.Search.Api/Services/SearchService.cs` | 148 |
| `Algocura.Search.Api/appsettings.json` | 45 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Search.Api/Controllers/SearchController.cs`

- L9: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L11: `[Route("api/v1/search")]`
- L16: `[HttpGet]`

### `Algocura.Search.Api/Controllers/SearchDocumentsController.cs`

- L11: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L13: `[Route("api/v1/search-documents")]`
- L18: `[HttpGet("{id:guid}")]`
- L36: `[HttpPost("{id:guid}/archive")]`
- L61: `[HttpPost("{id:guid}/restore")]`

### `Algocura.Search.Api/Controllers/SearchSynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/search/synchronization")]`
- L20: `[HttpPost]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Search.Api/Contracts/SearchContracts.cs`
Types: `SearchRequest`, `SearchTagResponse`, `SearchDocumentResponse`, `SearchResponse`

### `Algocura.Search.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.Search.Api/Data/Migrations/20260114025830_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocument", b =>`
- L132: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocumentTag", b =>`
- L158: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocumentTag", b =>`
- L169: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocument", b =>`

### `Algocura.Search.Api/Data/Migrations/20260114025830_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.Search.Api/Data/Migrations/20260806012829_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocument", b =>`
- L151: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocumentTag", b =>`
- L193: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocumentTag", b =>`
- L204: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocument", b =>`

### `Algocura.Search.Api/Data/Migrations/20260806012829_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.Search.Api/Data/Migrations/SearchDbContextModelSnapshot.cs`
Types: `SearchDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocument", b =>`
- L148: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocumentTag", b =>`
- L190: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocumentTag", b =>`
- L201: `modelBuilder.Entity("Algocura.Search.Api.Domain.SearchDocument", b =>`

### `Algocura.Search.Api/Data/SearchDbContext.cs`
Types: `SearchDbContext`
- L11: `public DbSet<SearchDocument> SearchDocuments =>`
- L14: `public DbSet<SearchDocumentTag> SearchDocumentTags =>`
- L19: `modelBuilder.Entity<SearchDocument>(entity =>`
- L21: `entity.HasKey(x => x.Id);`
- L22: `entity.Property(x => x.Id).ValueGeneratedNever();`
- L23: `entity.Property(x => x.Score).HasPrecision(18, 6);`
- L25: `entity.HasIndex(x => new`
- L31: `entity.HasIndex(x => x.Title);`
- L32: `entity.HasIndex(x => x.DiseaseName);`
- L33: `entity.HasIndex(x => x.GeneSymbol);`
- L34: `entity.HasIndex(x => x.CompoundName);`
- L35: `entity.HasIndex(x => x.InchiKey);`
- L36: `entity.HasIndex(x => x.WorkflowId);`
- L37: `entity.HasIndex(x => x.Status);`
- L39: `entity.HasMany(x => x.Tags)`
- L45: `modelBuilder.Entity<SearchDocumentTag>(entity =>`
- L47: `entity.HasKey(x => x.Id);`
- L48: `entity.Property(x => x.Id).ValueGeneratedNever();`
- L50: `entity.HasIndex(x => new`
- L65: `modelBuilder.Model`
- L75: `modelBuilder.Entity(entityType.ClrType);`
- L77: `entity.Property<Guid?>(`
- L81: `entity.Property<Guid?>(`
- L85: `entity.Property<DateTimeOffset>(`
- L89: `entity.Property<DateTimeOffset>(`
- L93: `entity.HasIndex(`
- L96: `entity.HasIndex(`

### `Algocura.Search.Api/Data/SearchDbContextFactory.cs`
Types: `SearchDbContextFactory`

### `Algocura.Search.Api/Data/SearchSeeder.cs`
Types: `SearchSeeder`, `describing`

### `Algocura.Search.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Search.Api/Messaging/Consumers/PlatformSearchEventsConsumer.cs`

### `Algocura.Search.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<SearchDbContext>(`
- L140 registration: `builder.Services.AddScoped<`
- L224 registration: `app.UseAuthentication();`
- L225 registration: `app.UseAuthorization();`
- L290 registration: `app.MapControllers();`

### `Algocura.Search.Api/Services/SearchDocumentFactory.cs`
- L114: `private static SearchDocumentType? MapDocumentType(string eventType)`

### `Algocura.Search.Api/Services/SearchService.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
