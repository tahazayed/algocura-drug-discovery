# Algocura.Identity.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 19.

## Original documented intent

# Algocura Identity API

The Identity API provides authentication, user management, role management,
and JWT bearer tokens for the Algocura Drug Discovery Platform.

## Responsibilities

- Register users
- Authenticate users
- Issue JWT access tokens
- Return the authenticated user's profile
- List users
- Retrieve users
- Assign roles
- Disable users
- Seed platform roles
- Seed the fixed system identity
- Optionally seed a development administrator
- Integrate with `Algocura.Platform.Auditing`
- Publish entity audit events through `Algocura.Platform.Messaging`
- Persist identity data in a dedicated SQL Server database


## Shared platform messaging

The Identity API uses `Algocura.Platform.Messaging` for Azure Service Bus
transport. Identity entity changes are audited by `Algocura.Platform.Auditing`
and published to the shared `algocura-events` topic.

```text
Identity entity change
        ↓
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
ServiceBusMessagePublisher
        ↓
Azure Service Bus
        ↓
Topic: algocura-events
        ↓
Audit API
```

Registration is centralized:

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

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

The Identity API does not require its own Service Bus subscription because it
publishes audit events but does not currently consume platform events.

The shared messaging project owns `ServiceBusClient`, `ServiceBusOptions`,
`IPlatformMessagePublisher`, `ServiceBusMessagePublisher`,
`IEventPublisher`, `ServiceBusEventPublisher`, and
`ServiceBusAuditEventPublisher`.

## Roles

The seed process creates:

```text
Administrator
Scientist
Reviewer
Service
```

## System identity

The fixed system identity is:

```text
00000000-0000-0000-0000-000000000001
```

It is stored as:

```text
system@algocura.local
```

and assigned the `Service` role.

## Development administrator

An administrator is only seeded when this configuration value is supplied:

```text
Seed:AdministratorPassword
```

The seeded administrator is:

```text
admin@algocura.local
```

Its fixed ID is:

```text
c1000000-0000-0000-0000-000000000001
```

Do not store a production administrator password in source-controlled
configuration. Supply it using environment variables, a secret store,
or another approved secret-management mechanism.

## Endpoints

```text
POST /api/v1/auth/register
POST /api/v1/auth/login
GET  /api/v1/auth/me

GET  /api/v1/users
GET  /api/v1/users/{id}
POST /api/v1/users/{id}/roles
POST /api/v1/users/{id}/disable

GET /health
GET /swagger
GET /redoc
GET /openapi/v1.json
```

The `/api/v1/users` endpoints require the `Administrator` role.

## Registration

Example request:

```json
{
  "email": "scientist@algocura.local",
  "password": "Example!Pass123",
  "displayName": "Algocura Scientist"
}
```

Newly registered users are assigned the `Scientist` role.

## Login

```json
{
  "email": "scientist@algocura.local",
  "password": "Example!Pass123"
}
```

The response contains a JWT access token and user profile.

## JWT claims

Tokens include:

```text
sub
email
ClaimTypes.NameIdentifier
ClaimTypes.Name
ClaimTypes.Role
```

## Password policy

```text
Minimum length: 12
Digit required
Uppercase required
Lowercase required
Non-alphanumeric required
Maximum failed attempts: 5
Lockout: 15 minutes
```

## Database migration

```powershell
Add-Migration InitialCreate `
    -Context IdentityDbContext `
    -OutputDir Data/Migrations

Update-Database `
    -Context IdentityDbContext
```

## Docker

Example environment configuration:

```yaml
identity-api:
  image: algocura/identity-api:dev
  build:
    context: .
    dockerfile: Algocura.Identity.Api/Dockerfile
  ports:
    - "7013:8080"
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__IdentityDatabase: >
      Server=sqlserver,1433;
      Database=AlgocuraIdentity;
      User Id=sa;
      Password=DontLogin!Passw0rd;
      TrustServerCertificate=True
    Jwt__Issuer: Algocura.Identity.Api
    Jwt__Audience: Algocura.Platform
    Jwt__SigningKey: ${ALGOCURA_JWT_SIGNING_KEY}
    Seed__AdministratorPassword: ${ALGOCURA_ADMIN_PASSWORD}
```

## Production direction

The current Identity API is a platform prototype that owns local ASP.NET
Core Identity users and issues symmetric-key JWTs.

For an enterprise deployment, authentication can later be federated with
Microsoft Entra ID while preserving Algocura application roles and user IDs
where appropriate.


## Project definition (authoritative build metadata)

Source: `Algocura.Identity.Api/Algocura.Identity.Api.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.10" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.10" />
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
| `Algocura.Identity.Api/Algocura.Identity.Api.csproj` | 39 |
| `Algocura.Identity.Api/Contracts/IdentityContracts.cs` | 26 |
| `Algocura.Identity.Api/Controllers/AuthController.cs` | 208 |
| `Algocura.Identity.Api/Controllers/UsersController.cs` | 144 |
| `Algocura.Identity.Api/Data/DatabaseInitializer.cs` | 88 |
| `Algocura.Identity.Api/Data/IdentityDbContext.cs` | 74 |
| `Algocura.Identity.Api/Data/IdentitySeeder.cs` | 346 |
| `Algocura.Identity.Api/Data/Migrations/20260120040744_UpdateLength.Designer.cs` | 326 |
| `Algocura.Identity.Api/Data/Migrations/20260120040744_UpdateLength.cs` | 260 |
| `Algocura.Identity.Api/Data/Migrations/IdentityDbContextModelSnapshot.cs` | 323 |
| `Algocura.Identity.Api/Data/SystemUsers.cs` | 10 |
| `Algocura.Identity.Api/Domain/ApplicationRole.cs` | 21 |
| `Algocura.Identity.Api/Domain/ApplicationUser.cs` | 25 |
| `Algocura.Identity.Api/Program.cs` | 457 |
| `Algocura.Identity.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Identity.Api/README.md` | 238 |
| `Algocura.Identity.Api/Security/IJwtTokenService.cs` | 11 |
| `Algocura.Identity.Api/Security/JwtTokenService.cs` | 116 |
| `Algocura.Identity.Api/appsettings.json` | 47 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Identity.Api/Controllers/AuthController.cs`

- L13: `[Route("api/v1/auth")]`
- L20: `[HttpPost("register")]`
- L21: `[AllowAnonymous]`
- L111: `[HttpPost("login")]`
- L112: `[AllowAnonymous]`
- L171: `[HttpGet("me")]`
- L172: `[Authorize]`

### `Algocura.Identity.Api/Controllers/UsersController.cs`

- L11: `[Authorize(Roles = "Administrator")]`
- L12: `[Route("api/v1/users")]`
- L17: `[HttpGet]`
- L49: `[HttpGet("{id:guid}")]`
- L76: `[HttpPost("{id:guid}/roles")]`
- L113: `[HttpPost("{id:guid}/disable")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Identity.Api/Contracts/IdentityContracts.cs`
Types: `RegisterRequest`, `LoginRequest`, `AssignRoleRequest`, `UserResponse`, `TokenResponse`
- L1: `namespace Algocura.Identity.Api.Contracts;`

### `Algocura.Identity.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`
- L3: `namespace Algocura.Identity.Api.Data;`

### `Algocura.Identity.Api/Data/IdentityDbContext.cs`
Types: `IdentityDbContext`
- L1: `using Algocura.Identity.Api.Domain;`
- L4: `using Microsoft.AspNetCore.Identity.EntityFrameworkCore;`
- L7: `namespace Algocura.Identity.Api.Data;`
- L25: `modelBuilder.Entity<ApplicationUser>(entity =>`
- L27: `entity.ToTable("Users");`
- L28: `entity.HasIndex(x => x.IsActive);`
- L31: `modelBuilder.Entity<ApplicationRole>()`
- L34: `modelBuilder.Entity<IdentityUserRole<Guid>>()`
- L37: `modelBuilder.Entity<IdentityUserClaim<Guid>>()`
- L40: `modelBuilder.Entity<IdentityUserLogin<Guid>>()`
- L43: `modelBuilder.Entity<IdentityRoleClaim<Guid>>()`
- L46: `modelBuilder.Entity<IdentityUserToken<Guid>>()`
- L49: `foreach (var entityType in modelBuilder.Model.GetEntityTypes()`
- L52: `var entity = modelBuilder.Entity(entityType.ClrType);`
- L54: `entity.Property<Guid?>(`
- L58: `entity.Property<Guid?>(`
- L62: `entity.Property<DateTimeOffset>(`
- L66: `entity.Property<DateTimeOffset>(`
- L70: `entity.HasIndex(nameof(IAuditableEntity.CreatedByUserId));`
- L71: `entity.HasIndex(nameof(IAuditableEntity.UpdatedByUserId));`

### `Algocura.Identity.Api/Data/IdentitySeeder.cs`
Types: `IdentitySeeder`
- L1: `using Algocura.Identity.Api.Domain;`
- L5: `namespace Algocura.Identity.Api.Data;`
- L55: `"Machine or service identity.",`

### `Algocura.Identity.Api/Data/Migrations/20260120040744_UpdateLength.Designer.cs`
Types: `UpdateLength`
- L3: `using Algocura.Identity.Api.Data;`
- L12: `namespace Algocura.Identity.Api.Data.Migrations`
- L28: `modelBuilder.Entity("Algocura.Identity.Api.Domain.ApplicationRole", b =>`
- L76: `modelBuilder.Entity("Algocura.Identity.Api.Domain.ApplicationUser", b =>`
- L170: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>`
- L194: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>`
- L218: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>`
- L239: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>`
- L254: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>`
- L273: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>`
- L275: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationRole", null)`
- L282: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>`
- L284: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`
- L291: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>`
- L293: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`
- L300: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>`
- L302: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationRole", null)`
- L308: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`
- L315: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>`
- L317: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`

### `Algocura.Identity.Api/Data/Migrations/20260120040744_UpdateLength.cs`
Types: `UpdateLength`
- L5: `namespace Algocura.Identity.Api.Data.Migrations`

### `Algocura.Identity.Api/Data/Migrations/IdentityDbContextModelSnapshot.cs`
Types: `IdentityDbContextModelSnapshot`
- L3: `using Algocura.Identity.Api.Data;`
- L11: `namespace Algocura.Identity.Api.Data.Migrations`
- L25: `modelBuilder.Entity("Algocura.Identity.Api.Domain.ApplicationRole", b =>`
- L73: `modelBuilder.Entity("Algocura.Identity.Api.Domain.ApplicationUser", b =>`
- L167: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>`
- L191: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>`
- L215: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>`
- L236: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>`
- L251: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>`
- L270: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>`
- L272: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationRole", null)`
- L279: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>`
- L281: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`
- L288: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>`
- L290: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`
- L297: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>`
- L299: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationRole", null)`
- L305: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`
- L312: `modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>`
- L314: `b.HasOne("Algocura.Identity.Api.Domain.ApplicationUser", null)`

### `Algocura.Identity.Api/Data/SystemUsers.cs`
Types: `SystemUsers`
- L1: `namespace Algocura.Identity.Api.Data;`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Identity.Api/Program.cs`
- L23 registration: `builder.Services.AddHealthChecks();`
- L36 registration: `builder.Services.AddCors(`
- L104 registration: `builder.Services.AddDbContext<IdentityDbContext>(`
- L182 registration: `builder.Services.AddScoped<`
- L279 registration: `app.UseAuthentication();`
- L280 registration: `app.UseAuthorization();`
- L345 registration: `app.MapControllers();`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
