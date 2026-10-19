# Algocura.CompoundCatalog.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 28.

## Original documented intent

# Algocura Compound Catalog API

The **Algocura Compound Catalog API** is a cloud-native RESTful microservice that serves as the authoritative source for chemical compound metadata within the Algocura Drug Discovery Platform.

It provides CRUD operations, search and filtering capabilities, publishes integration events through Azure Service Bus, and enables downstream microservices to consume consistent compound information.

For an overview of the complete platform architecture, see the [Solution README](../../README.md).

---

# Architecture

The Compound Catalog API manages the lifecycle of chemical compounds. It persists compound data in SQL Server using Entity Framework Core and publishes integration events through Azure Service Bus to notify downstream services of changes.

```text
                   Client Applications
                           │
                           ▼
                  ASP.NET Core API
                           │
                    CompoundsController
                           │
        ┌──────────────────┴──────────────────┐
        │                                     │
        ▼                                     ▼
 Entity Framework Core                 Integration Events
 (CompoundCatalogDbContext)             (IEventPublisher)
        │                                     │
        ▼                                     ▼
    SQL Server                      Azure Service Bus
                                              │
                                              ▼
                    Screening API
                    AI Discovery API
                    Workflow API
                    Notification API
```

---

# Features

- RESTful ASP.NET Core API
- OpenAPI / Swagger documentation
- Entity Framework Core with SQL Server
- Automatic database migrations
- Seed data initialization
- Compound search
- Status filtering
- Pagination
- Health checks
- Docker support
- Azure Service Bus integration
- Integration event publishing
- Structured logging

---

# Technology Stack

| Technology | Version |
|------------|----------|
| .NET | 10 |
| ASP.NET Core | 10 |
| Entity Framework Core | 10 |
| SQL Server | 2022 |
| Azure Service Bus | Emulator / Azure |
| Docker | Latest |
| OpenAPI | Microsoft.AspNetCore.OpenApi |

---

# Dependencies

This service depends on the following infrastructure components.

| Component | Purpose |
|-----------|---------|
| SQL Server | Persistent storage for compound data |
| Azure Service Bus | Integration event publishing |
| Docker | Local containerized development |
| OpenAPI | Interactive API documentation |

---

# Project Structure

```text
Algocura.CompoundCatalog.Api
│
├── Contracts
├── Controllers
├── Data
│   ├── CompoundCatalogDbContext
│   ├── CompoundCatalogSeeder
│   └── DatabaseInitializer
│
├── Domain
├── Events
├── Mapping
├── Messaging
├── OpenApi
├── Program.cs
├── Dockerfile
└── README.md
```

---

# Configuration

The service uses the standard ASP.NET Core configuration system.

Configuration is loaded from:

```text
appsettings.json
appsettings.Development.json
Environment Variables
```

Common configuration includes:

- SQL Server connection string
- Azure Service Bus configuration
- Logging
- OpenAPI
- Health Checks

Sensitive configuration should be provided through environment variables, Azure Key Vault, or .NET User Secrets during local development.

---

# Database

The service uses SQL Server with Entity Framework Core migrations.

During application startup the database is automatically initialized by:

```text
DatabaseInitializer
```

Initialization performs the following steps:

1. Apply pending Entity Framework Core migrations.
2. Seed reference compound data.
3. Start the application.

Reference data is seeded by:

```text
CompoundCatalogSeeder
```

---

# Compound Lifecycle

The catalog supports the following lifecycle states.

| Status | Description |
|---------|-------------|
| Draft | Initial registration |
| Registered | Available in the catalog |
| UnderEvaluation | Being evaluated |
| Selected | Selected for downstream workflows |
| Rejected | Evaluation failed |
| Archived | Historical record |

---

# API Overview

Base URL

```text
/api/v1/compounds
```

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/compounds` | Search compounds with filtering and pagination. |
| GET | `/api/v1/compounds/{id}` | Retrieve a compound by identifier. |
| POST | `/api/v1/compounds` | Register a new compound. |
| PUT | `/api/v1/compounds/{id}` | Update an existing compound. |
| DELETE | `/api/v1/compounds/{id}` | Delete a compound. |

## Query Parameters

### GET /api/v1/compounds

| Parameter | Type | Description |
|-----------|------|-------------|
| search | string | Search by Name, Canonical SMILES, InChIKey, or Molecular Formula. |
| status | CompoundStatus | Filter by lifecycle status. |
| skip | int | Number of records to skip. |
| take | int | Maximum number of records returned (default: 50). |

---

# OpenAPI

Interactive API documentation is available when the service is running locally.

| Resource | URL |
|----------|-----|
| Swagger UI | http://localhost:7001/swagger |
| OpenAPI Specification | http://localhost:7001/openapi/v1.json |
| Health Endpoint | http://localhost:7001/health |

The generated OpenAPI specification is the authoritative source for request and response schemas.

---

# Integration Events

The service publishes the following integration events through the configured `IEventPublisher`.

| Event | Description |
|------|-------------|
| CompoundRegistered | Published when a compound is created. |
| CompoundUpdated | Published when compound metadata is updated. |
| CompoundStatusChanged | Published when the lifecycle status changes. |
| CompoundDeleted | Published when a compound is removed. |

These events enable loose coupling between the Compound Catalog API and downstream microservices.

---

# Running Locally

Restore packages.

```bash
dotnet restore
```

Build the project.

```bash
dotnet build
```

Run the application.

```bash
dotnet run
```

---

# Running with Docker

Build the container.

```bash
docker compose build
```

Start the application and supporting infrastructure.

```bash
docker compose up -d
```

Stop the environment.

```bash
docker compose down
```

---

# Testing

Run all unit tests.

```bash
dotnet test
```

Current test coverage includes:

- Entity Framework Core DbContext
- Database initialization
- Seed data
- Controller endpoints

---

# Logging

The service uses `Microsoft.Extensions.Logging` for structured logging.

Logging includes:

- Application startup
- Database initialization
- Entity Framework Core migrations
- Compound creation
- Compound updates
- Compound deletion
- Integration event publication
- Unhandled exceptions

---

# Security

The current implementation is intended for local development.

Future enhancements include:

- Microsoft Entra ID authentication
- JWT Bearer authentication
- Role-based authorization
- API versioning
- Rate limiting
- Audit logging

---

# Future Enhancements

- Chemical structure similarity search
- Molecular fingerprint indexing
- Bulk import/export
- Versioned compounds
- Soft delete
- Optimistic concurrency
- Event replay
- GraphQL endpoint
- OData support
- Distributed caching
- Metrics and distributed tracing

---

# Related Documentation

| Document | Description |
|----------|-------------|
| [Solution README](../../README.md) | Overall platform architecture and microservices |
| OpenAPI Specification | http://localhost:7001/openapi/v1.json |

---

## Shared Platform Messaging

`Algocura.CompoundCatalog.Api` uses `Algocura.Platform.Messaging` for Azure Service Bus transport.

The API no longer owns local implementations of:

```text
IEventPublisher
ServiceBusEventPublisher
ServiceBusOptions
```

Publishing flows through the shared platform infrastructure:

```text
Compound Catalog domain event
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
        ↓
algocura-events
```

Audit events use the same transport:

```text
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
Azure Service Bus
```

The service keeps its Compound Catalog-specific event types, including:

```text
CompoundRegistered
CompoundUpdated
CompoundDeleted
CompoundStatusChanged
```

The local `Events/IntegrationEvent.cs` remains as an object-initializer-friendly base record, but now implements the shared `IIntegrationEvent` contract from `Algocura.Platform.Messaging.Events`.

### Registration

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

This shared registration provides:

```text
ServiceBusClient
IPlatformMessagePublisher -> ServiceBusMessagePublisher
IEventPublisher           -> ServiceBusEventPublisher
ServiceBusAuditEventPublisher
```

The API therefore must not register its own `ServiceBusClient`, `IEventPublisher`, or `ServiceBusEventPublisher`.

### Service Bus configuration

```json
"ServiceBus": {
  "UseEmulator": true,
  "ConnectionString": "...",
  "FullyQualifiedNamespace": "",
  "TopicName": "algocura-events",
  "DefaultMessageTimeToLiveDays": 7,
  "MaxConcurrentCalls": 4,
  "PrefetchCount": 0,
  "MaxAutoLockRenewalMinutes": 5
}
```

Compound Catalog is currently a publisher, so it does not require a Service Bus subscription.

### Auditing configuration

```json
"Auditing": {
  "ServiceName": "Algocura.CompoundCatalog.Api",
  "PublishEntityAuditEvents": true,
  "StampCreatedAndUpdatedFields": true,
  "IncludeUnchangedPropertiesOnCreate": true,
  "IncludeUnchangedPropertiesOnDelete": true,
  "IgnoreAuditInfrastructureEntities": true
}
```

---

# License

Copyright © Algocura Biotech LLC.

All rights reserved.

## Project definition (authoritative build metadata)

Source: `Algocura.CompoundCatalog.Api/Algocura.CompoundCatalog.Api.csproj`
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
| `Algocura.CompoundCatalog.Api/Algocura.CompoundCatalog.Api.csproj` | 39 |
| `Algocura.CompoundCatalog.Api/Contracts/CompoundResponse.cs` | 16 |
| `Algocura.CompoundCatalog.Api/Contracts/CreateCompoundRequest.cs` | 32 |
| `Algocura.CompoundCatalog.Api/Contracts/UpdateCompoundRequest.cs` | 32 |
| `Algocura.CompoundCatalog.Api/Controllers/CompoundSynchronizationController.cs` | 154 |
| `Algocura.CompoundCatalog.Api/Controllers/CompoundsController.cs` | 314 |
| `Algocura.CompoundCatalog.Api/Data/CompoundCatalogDbContext.cs` | 67 |
| `Algocura.CompoundCatalog.Api/Data/CompoundCatalogSeeder.cs` | 289 |
| `Algocura.CompoundCatalog.Api/Data/CompoundSeedData.cs` | 140 |
| `Algocura.CompoundCatalog.Api/Data/DatabaseInitializer.cs` | 130 |
| `Algocura.CompoundCatalog.Api/Data/Migrations/20260104042730_InitialSqlServerSchema.Designer.cs` | 92 |
| `Algocura.CompoundCatalog.Api/Data/Migrations/20260104042730_InitialSqlServerSchema.cs` | 59 |
| `Algocura.CompoundCatalog.Api/Data/Migrations/20260114043142_AddAuditMetadata.Designer.cs` | 102 |
| `Algocura.CompoundCatalog.Api/Data/Migrations/20260114043142_AddAuditMetadata.cs` | 56 |
| `Algocura.CompoundCatalog.Api/Data/Migrations/CompoundCatalogDbContextModelSnapshot.cs` | 99 |
| `Algocura.CompoundCatalog.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.CompoundCatalog.Api/Domain/Compound.cs` | 40 |
| `Algocura.CompoundCatalog.Api/Domain/CompoundStatus.cs` | 11 |
| `Algocura.CompoundCatalog.Api/Events/CompoundDeleted.cs` | 8 |
| `Algocura.CompoundCatalog.Api/Events/CompoundRegistered.cs` | 20 |
| `Algocura.CompoundCatalog.Api/Events/CompoundStatusChanged.cs` | 14 |
| `Algocura.CompoundCatalog.Api/Events/CompoundUpdated.cs` | 20 |
| `Algocura.CompoundCatalog.Api/Events/IntegrationEvent.cs` | 22 |
| `Algocura.CompoundCatalog.Api/Mapping/CompoundMappings.cs` | 23 |
| `Algocura.CompoundCatalog.Api/Program.cs` | 397 |
| `Algocura.CompoundCatalog.Api/Properties/launchSettings.json` | 23 |
| `Algocura.CompoundCatalog.Api/README.md` | 456 |
| `Algocura.CompoundCatalog.Api/appsettings.json` | 43 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.CompoundCatalog.Api/Controllers/CompoundsController.cs`

- L16: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L18: `[Route("api/v1/compounds")]`
- L36: `[HttpGet]`
- L40: `[ProducesResponseType<IReadOnlyCollection<CompoundResponse>>(`
- L86: `[HttpGet("{id:guid}")]`
- L88: `[ProducesResponseType<CompoundResponse>(StatusCodes.Status200OK)]`
- L89: `[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]`
- L114: `[HttpPost]`
- L116: `[ProducesResponseType<CompoundResponse>(StatusCodes.Status201Created)]`
- L117: `[ProducesResponseType<ValidationProblemDetails>(`
- L119: `[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]`
- L188: `[HttpPut("{id:guid}")]`
- L190: `[ProducesResponseType<CompoundResponse>(StatusCodes.Status200OK)]`
- L191: `[ProducesResponseType<ValidationProblemDetails>(`
- L193: `[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]`
- L194: `[ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]`
- L274: `[HttpDelete("{id:guid}")]`
- L276: `[ProducesResponseType(StatusCodes.Status204NoContent)]`
- L277: `[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]`

### `Algocura.CompoundCatalog.Api/Controllers/CompoundSynchronizationController.cs`

- L10: `[Authorize(Roles = "Service,Administrator")]`
- L12: `[Route("api/v1/compounds/synchronization")]`
- L26: `[HttpPost]`
- L27: `[ProducesResponseType<CompoundSynchronizationResponse>(`
- L29: `[ProducesResponseType<ProblemDetails>(`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.CompoundCatalog.Api/Contracts/CompoundResponse.cs`
Types: `CompoundResponse`

### `Algocura.CompoundCatalog.Api/Contracts/CreateCompoundRequest.cs`
Types: `CreateCompoundRequest`
- L17: `public string? InchiKey { get; init; }`
- L20: `public decimal MolecularWeight { get; init; }`
- L23: `public string? MolecularFormula { get; init; }`
- L26: `public string? Source { get; init; }`
- L29: `public string? Description { get; init; }`
- L31: `public CompoundStatus Status { get; init; } = CompoundStatus.Draft;`

### `Algocura.CompoundCatalog.Api/Contracts/UpdateCompoundRequest.cs`
Types: `UpdateCompoundRequest`
- L17: `public string? InchiKey { get; init; }`
- L20: `public decimal MolecularWeight { get; init; }`
- L23: `public string? MolecularFormula { get; init; }`
- L26: `public string? Source { get; init; }`
- L29: `public string? Description { get; init; }`
- L31: `public CompoundStatus Status { get; init; }`

### `Algocura.CompoundCatalog.Api/Data/CompoundCatalogDbContext.cs`
Types: `CompoundCatalogDbContext`
- L10: `public DbSet<Compound> Compounds => Set<Compound>();`
- L16: `var compound = modelBuilder.Entity<Compound>();`

### `Algocura.CompoundCatalog.Api/Data/CompoundCatalogSeeder.cs`
Types: `CompoundCatalogSeeder`

### `Algocura.CompoundCatalog.Api/Data/CompoundSeedData.cs`
Types: `CompoundSeedData`

### `Algocura.CompoundCatalog.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.CompoundCatalog.Api/Data/Migrations/20260104042730_InitialSqlServerSchema.Designer.cs`
Types: `InitialSqlServerSchema`
- L28: `modelBuilder.Entity("Algocura.CompoundCatalog.Api.Domain.Compound", b =>`

### `Algocura.CompoundCatalog.Api/Data/Migrations/20260104042730_InitialSqlServerSchema.cs`
Types: `InitialSqlServerSchema`

### `Algocura.CompoundCatalog.Api/Data/Migrations/20260114043142_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.CompoundCatalog.Api.Domain.Compound", b =>`

### `Algocura.CompoundCatalog.Api/Data/Migrations/20260114043142_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.CompoundCatalog.Api/Data/Migrations/CompoundCatalogDbContextModelSnapshot.cs`
Types: `CompoundCatalogDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.CompoundCatalog.Api.Domain.Compound", b =>`

### `Algocura.CompoundCatalog.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.CompoundCatalog.Api/Program.cs`
- L20 registration: `builder.Services.AddHealthChecks();`
- L33 registration: `builder.Services.AddCors(`
- L101 registration: `builder.Services.AddDbContext<CompoundCatalogDbContext>(`
- L222 registration: `app.UseAuthentication();`
- L223 registration: `app.UseAuthorization();`
- L288 registration: `app.MapControllers();`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
