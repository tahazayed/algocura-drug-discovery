# Algocura.Notification.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 24.

## Original documented intent

# Algocura Notification API

The Notification API provides cross-cutting notification management for
the Algocura Drug Discovery Platform.

It subscribes to `algocura-events`, applies configurable notification
rules, creates delivery records, dispatches notifications through a
prototype provider, records delivery attempts, and publishes delivery
status events.

## Prototype limitation

`LoggingNotificationDispatcher` logs notification delivery and returns a
simulated provider message ID. It does not send real email, Microsoft
Teams messages, or webhooks.

Replace it with approved provider adapters such as Microsoft Graph,
Azure Communication Services, SendGrid, or Teams webhooks.

## Local URLs

- `http://localhost:7008/`
- `http://localhost:7008/swagger`
- `http://localhost:7008/redoc`
- `http://localhost:7008/openapi/v1.json`
- `http://localhost:7008/health`

## Routes

- `GET /api/v1/notification-rules`
- `GET /api/v1/notification-rules/{id}`
- `POST /api/v1/notification-rules`
- `PUT /api/v1/notification-rules/{id}`
- `DELETE /api/v1/notification-rules/{id}`
- `GET /api/v1/notifications`
- `GET /api/v1/notifications/{id}`
- `POST /api/v1/notifications`
- `POST /api/v1/notifications/{id}/dispatch`
- `POST /api/v1/notifications/{id}/cancel`
- `POST /api/v1/notification/synchronization`

## Events consumed

The service can consume any event when an enabled rule exists for its
`eventType`, including `TargetSelected`, `StructureSearchCompleted`,
`ScreeningCompleted`, `AssayCompleted`, and `AssaySnapshot`.

## Events published

- `NotificationDelivered`
- `NotificationDeliveryFailed`
- `NotificationSnapshot`

## Migrations

```powershell
Add-Migration InitialCreate `
  -Context NotificationDbContext `
  -OutputDir Data/Migrations

Update-Database `
  -Context NotificationDbContext
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

Source: `Algocura.Notification.Api/Algocura.Notification.Api.csproj`
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
| `Algocura.Notification.Api/Algocura.Notification.Api.csproj` | 25 |
| `Algocura.Notification.Api/Contracts/Contracts.cs` | 60 |
| `Algocura.Notification.Api/Controllers/NotificationRulesController.cs` | 116 |
| `Algocura.Notification.Api/Controllers/NotificationSynchronizationController.cs` | 129 |
| `Algocura.Notification.Api/Controllers/NotificationsController.cs` | 243 |
| `Algocura.Notification.Api/Data/DatabaseInitializer.cs` | 84 |
| `Algocura.Notification.Api/Data/Migrations/20260113052855_InitialCreate.Designer.cs` | 270 |
| `Algocura.Notification.Api/Data/Migrations/20260113052855_InitialCreate.cs` | 162 |
| `Algocura.Notification.Api/Data/Migrations/20260806021125_AddAuditMetadata.Designer.cs` | 337 |
| `Algocura.Notification.Api/Data/Migrations/20260806021125_AddAuditMetadata.cs` | 262 |
| `Algocura.Notification.Api/Data/Migrations/NotificationDbContextModelSnapshot.cs` | 334 |
| `Algocura.Notification.Api/Data/NotificationDbContext.cs` | 104 |
| `Algocura.Notification.Api/Data/NotificationDbContextFactory.cs` | 19 |
| `Algocura.Notification.Api/Data/NotificationSeeder.cs` | 624 |
| `Algocura.Notification.Api/Data/SystemUsers.cs` | 7 |
| `Algocura.Notification.Api/Domain/Entities.cs` | 202 |
| `Algocura.Notification.Api/Events/IntegrationEvents.cs` | 38 |
| `Algocura.Notification.Api/Mapping/Mappings.cs` | 28 |
| `Algocura.Notification.Api/Messaging/Consumers/PlatformEventsConsumer.cs` | 185 |
| `Algocura.Notification.Api/Program.cs` | 406 |
| `Algocura.Notification.Api/Properties/launchSettings.json` | 23 |
| `Algocura.Notification.Api/README.md` | 94 |
| `Algocura.Notification.Api/Services/NotificationServices.cs` | 111 |
| `Algocura.Notification.Api/appsettings.json` | 45 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Notification.Api/Controllers/NotificationRulesController.cs`

- L11: `[Authorize(Roles = "Administrator")]`
- L13: `[Route("api/v1/notification-rules")]`
- L17: `[HttpGet]`
- L35: `[HttpGet("{id:guid}")]`
- L46: `[HttpPost]`
- L77: `[HttpPut("{id:guid}")]`
- L102: `[HttpDelete("{id:guid}")]`

### `Algocura.Notification.Api/Controllers/NotificationsController.cs`

- L14: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L16: `[Route("api/v1/notifications")]`
- L22: `[HttpGet]`
- L51: `[HttpGet("{id:guid}")]`
- L60: `[HttpPost]`
- L104: `[HttpPost("{id:guid}/dispatch")]`
- L207: `[HttpPost("{id:guid}/cancel")]`

### `Algocura.Notification.Api/Controllers/NotificationSynchronizationController.cs`

- L11: `[Authorize(Roles = "Service,Administrator")]`
- L13: `[Route("api/v1/notification/synchronization")]`
- L20: `[HttpPost]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Notification.Api/Contracts/Contracts.cs`
Types: `CreateNotificationRuleRequest`, `UpdateNotificationRuleRequest`, `CreateManualNotificationRequest`, `NotificationRuleResponse`, `NotificationDeliveryAttemptResponse`, `NotificationDeliveryResponse`, `NotificationResponse`

### `Algocura.Notification.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.Notification.Api/Data/Migrations/20260113052855_InitialCreate.Designer.cs`
Types: `InitialCreate`
- L28: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L89: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L136: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDeliveryAttempt", b =>`
- L169: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationRule", b =>`
- L221: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L231: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L242: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDeliveryAttempt", b =>`
- L253: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L258: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L263: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationRule", b =>`

### `Algocura.Notification.Api/Data/Migrations/20260113052855_InitialCreate.cs`
Types: `InitialCreate`

### `Algocura.Notification.Api/Data/Migrations/20260806021125_AddAuditMetadata.Designer.cs`
Types: `AddAuditMetadata`
- L28: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L111: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L174: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDeliveryAttempt", b =>`
- L223: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationRule", b =>`
- L288: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L298: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L309: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDeliveryAttempt", b =>`
- L320: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L325: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L330: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationRule", b =>`

### `Algocura.Notification.Api/Data/Migrations/20260806021125_AddAuditMetadata.cs`
Types: `AddAuditMetadata`

### `Algocura.Notification.Api/Data/Migrations/NotificationDbContextModelSnapshot.cs`
Types: `NotificationDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L108: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L171: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDeliveryAttempt", b =>`
- L220: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationRule", b =>`
- L285: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L295: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L306: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDeliveryAttempt", b =>`
- L317: `modelBuilder.Entity("Algocura.Notification.Api.Domain.Notification", b =>`
- L322: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationDelivery", b =>`
- L327: `modelBuilder.Entity("Algocura.Notification.Api.Domain.NotificationRule", b =>`

### `Algocura.Notification.Api/Data/NotificationDbContext.cs`
Types: `NotificationDbContext`
- L10: `public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();`
- L11: `public DbSet<Algocura.Notification.Api.Domain.Notification> Notifications => Set<Algocura.Notification.Api.Domain.Notification>();`
- L12: `public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();`
- L13: `public DbSet<NotificationDeliveryAttempt> NotificationDeliveryAttempts => Set<NotificationDeliveryAttempt>();`
- L17: `modelBuilder.Entity<NotificationRule>(e =>`
- L26: `modelBuilder.Entity<Algocura.Notification.Api.Domain.Notification>(e =>`
- L36: `modelBuilder.Entity<NotificationDelivery>(e =>`
- L45: `modelBuilder.Entity<NotificationDeliveryAttempt>(e =>`
- L60: `modelBuilder.Model`
- L72: `modelBuilder.Entity(`
- L75: `entity.Property<Guid?>(`
- L80: `entity.Property<Guid?>(`
- L85: `entity.Property<DateTimeOffset>(`
- L90: `entity.Property<DateTimeOffset>(`
- L95: `entity.HasIndex(`
- L99: `entity.HasIndex(`

### `Algocura.Notification.Api/Data/NotificationDbContextFactory.cs`
Types: `NotificationDbContextFactory`

### `Algocura.Notification.Api/Data/NotificationSeeder.cs`
Types: `NotificationSeeder`

### `Algocura.Notification.Api/Data/SystemUsers.cs`
Types: `SystemUsers`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Notification.Api/Messaging/Consumers/PlatformEventsConsumer.cs`

### `Algocura.Notification.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<NotificationDbContext>(`
- L140 registration: `builder.Services.AddScoped<`
- L144 registration: `builder.Services.AddScoped<`
- L228 registration: `app.UseAuthentication();`
- L229 registration: `app.UseAuthorization();`
- L294 registration: `app.MapControllers();`

### `Algocura.Notification.Api/Services/NotificationServices.cs`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
