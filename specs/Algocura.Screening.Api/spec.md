# Algocura.Screening.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 27.

## Original documented intent

# Algocura Screening API

A .NET 10 microservice that creates and tracks virtual-screening jobs, maintains a local projection of compounds received from the Compound Catalog API, persists screening state in SQL Server through EF Core, and publishes lifecycle events through Azure Service Bus.

## Projects

- `Algocura.Screening.Api`: ASP.NET Core controller API, EF Core SQL Server persistence, Service Bus publisher and consumer.
- `Algocura.Screening.Api.Tests`: xUnit unit tests using EF Core InMemory, FluentAssertions, and NSubstitute.

## API

Base URL: `/api/v1/screenings`

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/v1/screenings` | List screenings with status and paging filters |
| GET | `/api/v1/screenings/{id}` | Get one screening |
| POST | `/api/v1/screenings` | Create a queued screening |
| PUT | `/api/v1/screenings/{id}` | Edit a draft or queued screening |
| POST | `/api/v1/screenings/{id}/start` | Start a queued screening |
| POST | `/api/v1/screenings/{id}/complete` | Complete and rank results |
| POST | `/api/v1/screenings/{id}/fail` | Mark queued/running screening failed |
| POST | `/api/v1/screenings/{id}/cancel` | Cancel a nonterminal screening |
| DELETE | `/api/v1/screenings/{id}` | Delete a non-running screening |

## Events

Consumes `CompoundRegisteredMessage`, `CompoundUpdatedMessage`, and `CompoundDeletedMessage` from `compound-events` / `screening-api`.

Publishes `ScreeningCreated`, `ScreeningStarted`, `ScreeningCompleted`, `ScreeningFailed`, and `ScreeningCancelled` to `screening-events`.

## Commands

```bash
dotnet restore Algocura.Screening.slnx
dotnet build Algocura.Screening.slnx
dotnet test Algocura.Screening.slnx
```

Create a migration after changing the model:

```bash
dotnet ef migrations add MigrationName --project Algocura.Screening.Api --startup-project Algocura.Screening.Api
```

Run locally:

```bash
dotnet run --project Algocura.Screening.Api
```

Swagger: `http://localhost:7002/swagger`
Health: `http://localhost:7002/health`

## Important integration contract note

The compound event record names and fields must exactly match the events published by `Algocura.CompoundCatalog.Api`. Move shared event contracts into a versioned contracts package when the second service is integrated.

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

Source: `Algocura.Screening.Api/Algocura.Screening.Api.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk.Web">
	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
		<InvariantGlobalization>false</InvariantGlobalization>
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

		<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.10">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>

		<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="10.0.10" />

		<PackageReference Include="Microsoft.OpenApi" Version="2.7.5" />

		<PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" Version="10.2.3" />

		<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.2.3" />

		<PackageReference Include="Swashbuckle.AspNetCore.ReDoc" Version="10.2.3" />

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
| `Algocura.Screening.Api/Algocura.Screening.Api.csproj` | 51 |
| `Algocura.Screening.Api/Contracts/ScreeningContracts.cs` | 25 |
| `Algocura.Screening.Api/Controllers/ScreeningSynchronizationController.cs` | 192 |
| `Algocura.Screening.Api/Controllers/ScreeningsController.cs` | 206 |
| `Algocura.Screening.Api/Data/DatabaseInitializer.cs` | 86 |
| `Algocura.Screening.Api/Data/Migrations/20260730074716_InitialCreate.Designer.cs` | 209 |
| `Algocura.Screening.Api/Data/Migrations/20260730074716_InitialCreate.cs` | 139 |
| `Algocura.Screening.Api/Data/Migrations/20260806014211_AddAuditMetadata.Designer.cs` | 282 |
| `Algocura.Screening.Api/Data/Migrations/20260806014211_AddAuditMetadata.cs` | 284 |
| `Algocura.Screening.Api/Data/Migrations/ScreeningDbContextModelSnapshot.cs` | 279 |
| `Algocura.Screening.Api/Data/ScreeningDbContext.cs` | 110 |
| `Algocura.Screening.Api/Data/ScreeningSeeder.cs` | 532 |
| `Algocura.Screening.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.Screening.Api/Domain/ScreeningCandidate.cs` | 29 |
| `Algocura.Screening.Api/Domain/ScreeningCompound.cs` | 30 |
| `Algocura.Screening.Api/Domain/ScreeningJob.cs` | 44 |
| `Algocura.Screening.Api/Domain/ScreeningResult.cs` | 35 |
| `Algocura.Screening.Api/Domain/ScreeningStatus.cs` | 11 |
| `Algocura.Screening.Api/Events/CompletedScreeningSnapshot.cs` | 42 |
| `Algocura.Screening.Api/Events/CompoundIntegrationEvents.cs` | 81 |
| `Algocura.Screening.Api/Events/IntegrationEvents.cs` | 121 |
| `Algocura.Screening.Api/Mapping/ScreeningMapping.cs` | 13 |
| `Algocura.Screening.Api/Messaging/Consumers/CompoundEventsConsumer.cs` | 328 |
| `Algocura.Screening.Api/Program.cs` | 394 |
| `Algocura.Screening.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Screening.Api/README.md` | 86 |
| `Algocura.Screening.Api/appsettings.json` | 43 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Screening.Api/Controllers/ScreeningsController.cs`

- L13: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L15: `[Route("api/v1/screenings")]`
- L16: `public sealed class ScreeningsController(ScreeningDbContext db, IEventPublisher publisher, ILogger<ScreeningsController> logger) : ControllerBase`
- L18: `[HttpGet]`
- L19: `[ProducesResponseType<IReadOnlyCollection<ScreeningResponse>>(StatusCodes.Status200OK)]`
- L20: `public async Task<ActionResult<IReadOnlyCollection<ScreeningResponse>>> GetAll([FromQuery] ScreeningStatus? status, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)`
- L30: `[HttpGet("{id:guid}")]`
- L31: `[ProducesResponseType<ScreeningResponse>(StatusCodes.Status200OK)]`
- L32: `[ProducesResponseType(StatusCodes.Status404NotFound)]`
- L33: `public async Task<ActionResult<ScreeningResponse>> GetById(Guid id, CancellationToken cancellationToken = default)`
- L39: `[HttpPost]`
- L40: `[ProducesResponseType<ScreeningResponse>(StatusCodes.Status201Created)]`
- L41: `[ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]`
- L42: `public async Task<ActionResult<ScreeningResponse>> Create(CreateScreeningRequest request, CancellationToken cancellationToken = default)`
- L85: `[HttpPut("{id:guid}")]`
- L86: `public async Task<ActionResult<ScreeningResponse>> Update(Guid id, UpdateScreeningRequest request, CancellationToken cancellationToken = default)`
- L100: `[HttpPost("{id:guid}/start")]`
- L101: `public async Task<ActionResult<ScreeningResponse>> Start(Guid id, CancellationToken cancellationToken = default)`
- L118: `[HttpPost("{id:guid}/complete")]`
- L119: `public async Task<ActionResult<ScreeningResponse>> Complete(Guid id, CompleteScreeningRequest request, CancellationToken cancellationToken = default)`
- L155: `[HttpPost("{id:guid}/fail")]`
- L156: `public async Task<ActionResult<ScreeningResponse>> Fail(Guid id, FailScreeningRequest request, CancellationToken cancellationToken = default)`
- L175: `[HttpPost("{id:guid}/cancel")]`
- L176: `public async Task<ActionResult<ScreeningResponse>> Cancel(Guid id, CancellationToken cancellationToken = default)`
- L193: `[HttpDelete("{id:guid}")]`
- L194: `public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)`
- L204: `private Task<ScreeningJob?> LoadJobAsync(Guid id, CancellationToken ct) => db.ScreeningJobs.Include(x => x.Candidates).Include(x => x.Results).SingleOrDefaultAsync(x => x.Id == id, ct);`
- L205: `private ConflictObjectResult InvalidTransition(ScreeningStatus current, ScreeningStatus requested) => Conflict(new ProblemDetails { Title = "Invalid status transition", Detail = $"Cannot transition screening from {current} to {requested}." });`

### `Algocura.Screening.Api/Controllers/ScreeningSynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/screenings/synchronization")]`
- L20: `[HttpPost]`
- L96: `[HttpPost("{id:guid}")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Screening.Api/Contracts/ScreeningContracts.cs`
Types: `CreateScreeningRequest`, `UpdateScreeningRequest`, `CompleteScreeningRequest`, `ScreeningResultInput`, `FailScreeningRequest`, `ScreeningCandidateResponse`, `ScreeningResultResponse`, `ScreeningResponse`

### `Algocura.Screening.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.Screening.Api/Data/Migrations/20260730074716_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCandidate", b =>`
- L61: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCompound", b =>`
- L94: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningJob", b =>`
- L141: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningResult", b =>`
- L178: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCandidate", b =>`
- L189: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningResult", b =>`
- L200: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningJob", b =>`

### `Algocura.Screening.Api/Data/Migrations/20260730074716_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.Screening.Api/Data/Migrations/20260806014211_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCandidate", b =>`
- L77: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCompound", b =>`
- L129: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningJob", b =>`
- L195: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningResult", b =>`
- L251: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCandidate", b =>`
- L262: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningResult", b =>`
- L273: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningJob", b =>`

### `Algocura.Screening.Api/Data/Migrations/20260806014211_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.Screening.Api/Data/Migrations/ScreeningDbContextModelSnapshot.cs`
Types: `ScreeningDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCandidate", b =>`
- L74: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCompound", b =>`
- L126: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningJob", b =>`
- L192: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningResult", b =>`
- L248: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningCandidate", b =>`
- L259: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningResult", b =>`
- L270: `modelBuilder.Entity("Algocura.Screening.Api.Domain.ScreeningJob", b =>`

### `Algocura.Screening.Api/Data/ScreeningDbContext.cs`
Types: `ScreeningDbContext`
- L9: `public DbSet<ScreeningJob> ScreeningJobs => Set<ScreeningJob>();`
- L10: `public DbSet<ScreeningCandidate> ScreeningCandidates => Set<ScreeningCandidate>();`
- L11: `public DbSet<ScreeningResult> ScreeningResults => Set<ScreeningResult>();`
- L12: `public DbSet<ScreeningCompound> ScreeningCompounds => Set<ScreeningCompound>();`
- L16: `modelBuilder.Entity<ScreeningJob>(entity =>`
- L18: `entity.ToTable("ScreeningJobs");`
- L19: `entity.HasKey(x => x.Id);`
- L20: `entity.Property(x => x.Name).HasMaxLength(200).IsRequired();`
- L21: `entity.Property(x => x.Description).HasMaxLength(2000);`
- L22: `entity.Property(x => x.ScreeningMethod).HasMaxLength(100).IsRequired();`
- L23: `entity.Property(x => x.Status).HasConversion<int>();`
- L24: `entity.HasIndex(x => x.Status);`
- L25: `entity.HasIndex(x => x.CreatedAtUtc);`
- L28: `modelBuilder.Entity<ScreeningCandidate>(entity =>`
- L30: `entity.ToTable("ScreeningCandidates");`
- L31: `entity.HasKey(x => x.Id);`
- L32: `entity.Property(x => x.CompoundName).HasMaxLength(250).IsRequired();`
- L33: `entity.Property(x => x.CanonicalSmiles).HasMaxLength(4000).IsRequired();`
- L34: `entity.HasIndex(x => new { x.ScreeningJobId, x.CompoundId }).IsUnique();`
- L35: `entity.HasOne(x => x.ScreeningJob).WithMany(x => x.Candidates).HasForeignKey(x => x.ScreeningJobId).OnDelete(DeleteBehavior.Cascade);`
- L38: `modelBuilder.Entity<ScreeningResult>(entity =>`
- L40: `entity.ToTable("ScreeningResults");`
- L41: `entity.HasKey(x => x.Id);`
- L42: `entity.Property(x => x.Score).HasPrecision(18, 8);`
- L43: `entity.Property(x => x.Notes).HasMaxLength(2000);`
- L44: `entity.HasIndex(x => new { x.ScreeningJobId, x.CompoundId }).IsUnique();`
- L45: `entity.HasOne(x => x.ScreeningJob).WithMany(x => x.Results).HasForeignKey(x => x.ScreeningJobId).OnDelete(DeleteBehavior.Cascade);`
- L48: `modelBuilder.Entity<ScreeningCompound>(entity =>`
- L50: `entity.ToTable("ScreeningCompounds");`
- L51: `entity.HasKey(x => x.CompoundId);`
- L52: `entity.Property(x => x.Name).HasMaxLength(250).IsRequired();`
- L53: `entity.Property(x => x.CanonicalSmiles).HasMaxLength(4000).IsRequired();`
- L54: `entity.Property(x => x.InchiKey).HasMaxLength(27);`
- L55: `entity.HasIndex(x => x.InchiKey);`
- L66: `modelBuilder.Model`
- L78: `modelBuilder.Entity(`
- L81: `entity.Property<Guid?>(`
- L86: `entity.Property<Guid?>(`
- L91: `entity.Property<DateTimeOffset>(`
- L96: `entity.Property<DateTimeOffset>(`
- L101: `entity.HasIndex(`
- L105: `entity.HasIndex(`

### `Algocura.Screening.Api/Data/ScreeningSeeder.cs`
Types: `ScreeningSeeder`

### `Algocura.Screening.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Screening.Api/Messaging/Consumers/CompoundEventsConsumer.cs`

### `Algocura.Screening.Api/Program.cs`
- L21 registration: `builder.Services.AddHealthChecks();`
- L34 registration: `builder.Services.AddCors(`
- L102 registration: `builder.Services.AddDbContext<ScreeningDbContext>(`
- L216 registration: `app.UseAuthentication();`
- L217 registration: `app.UseAuthorization();`
- L282 registration: `app.MapControllers();`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
