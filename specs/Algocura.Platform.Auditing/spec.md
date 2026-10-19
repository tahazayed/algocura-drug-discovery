# Algocura.Platform.Auditing — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 21.

## Original documented intent

# Algocura.Platform.Auditing

Shared auditing library for Algocura services.

## Included components

- `IAuditableEntity`
- `IOwnedEntity`
- `IApprovedEntity`
- `ICompletedEntity`
- `IProjectionAuditMetadata`
- `ICurrentUserContext`
- `CurrentUserContext`
- `AuditableEntityInterceptor`
- `EntityAuditEventInterceptor`
- `EntityAuditEvent`
- `IAuditEventPublisher`
- `IAuditValueSanitizer`
- `DefaultAuditValueSanitizer`
- Dependency-injection extensions

## Register the library

```csharp
builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName =
        "Algocura.CompoundCatalog.Api";

    options.PublishEntityAuditEvents =
        true;
});
```

For a custom publisher:

```csharp
builder.Services.AddAlgocuraAuditing<
    ServiceBusAuditEventPublisher>(
    options =>
    {
        options.ServiceName =
            "Algocura.CompoundCatalog.Api";
    });
```

## Add the interceptors to a DbContext

```csharp
builder.Services.AddDbContext<CompoundCatalogDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            databaseConnectionString);

        options.AddInterceptors(
            serviceProvider.GetRequiredService<
                AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<
                EntityAuditEventInterceptor>());
    });
```

## Implement the auditable interface

```csharp
public sealed class Compound : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
```

## Identity source

`CurrentUserContext` reads the current authenticated identity from:

- `sub`
- `ClaimTypes.NameIdentifier`
- `oid`

It also reads:

- display name
- email
- correlation ID
- trace ID
- IP address
- user agent

Do not accept audit user IDs from request bodies.

## Sensitive properties

The default sanitizer excludes:

- passwords
- access tokens
- refresh tokens
- client secrets
- connection strings
- shared access keys
- API keys

It masks:

- email
- recipient
- IP address
- user agent

Extend these collections through `AuditingOptions`.

## Important transaction note

`EntityAuditEventInterceptor` publishes after the database save succeeds.

This avoids publishing audit events for failed writes, but it does not
provide atomic database-and-message consistency. For production-grade
guarantees, implement the transactional outbox pattern in each service
and publish audit events from the outbox.


## Project definition (authoritative build metadata)

Source: `Algocura.Platform.Auditing/Algocura.Platform.Auditing.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
		<GenerateDocumentationFile>true</GenerateDocumentationFile>
		<RootNamespace>Algocura.Platform.Auditing</RootNamespace>
		<NoWarn>$(NoWarn);1591</NoWarn>
	</PropertyGroup>

	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>

	<ItemGroup>
		<PackageReference
		  Include="Microsoft.EntityFrameworkCore"
		  Version="10.0.10" />
	</ItemGroup>

</Project>
```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Platform.Auditing/Abstractions/IApprovedEntity.cs` | 11 |
| `Algocura.Platform.Auditing/Abstractions/IAuditEventPublisher.cs` | 14 |
| `Algocura.Platform.Auditing/Abstractions/IAuditValueSanitizer.cs` | 12 |
| `Algocura.Platform.Auditing/Abstractions/IAuditableEntity.cs` | 15 |
| `Algocura.Platform.Auditing/Abstractions/ICompletedEntity.cs` | 12 |
| `Algocura.Platform.Auditing/Abstractions/ICurrentUserContext.cs` | 28 |
| `Algocura.Platform.Auditing/Abstractions/IOwnedEntity.cs` | 10 |
| `Algocura.Platform.Auditing/Abstractions/IProjectionAuditMetadata.cs` | 12 |
| `Algocura.Platform.Auditing/Algocura.Platform.Auditing.csproj` | 22 |
| `Algocura.Platform.Auditing/DependencyInjection/AuditingServiceCollectionExtensions.cs` | 66 |
| `Algocura.Platform.Auditing/Events/EntityAuditEvent.cs` | 49 |
| `Algocura.Platform.Auditing/Identity/CurrentUserContext.cs` | 89 |
| `Algocura.Platform.Auditing/Interceptors/AuditableEntityInterceptor.cs` | 85 |
| `Algocura.Platform.Auditing/Interceptors/EntityAuditEventInterceptor.cs` | 376 |
| `Algocura.Platform.Auditing/Models/AuditAction.cs` | 15 |
| `Algocura.Platform.Auditing/Models/AuditActorType.cs` | 9 |
| `Algocura.Platform.Auditing/Models/PendingAuditEntry.cs` | 25 |
| `Algocura.Platform.Auditing/Options/AuditingOptions.cs` | 50 |
| `Algocura.Platform.Auditing/Publishing/NullAuditEventPublisher.cs` | 20 |
| `Algocura.Platform.Auditing/README.md` | 128 |
| `Algocura.Platform.Auditing/Sanitization/DefaultAuditValueSanitizer.cs` | 64 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Platform.Auditing/Models/AuditAction.cs`
Types: `AuditAction`

### `Algocura.Platform.Auditing/Models/AuditActorType.cs`
Types: `AuditActorType`

### `Algocura.Platform.Auditing/Models/PendingAuditEntry.cs`
Types: `PendingAuditEntry`
- L19: `public HashSet<string> ChangedProperties { get; } =`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
