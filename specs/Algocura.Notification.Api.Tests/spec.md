# Algocura.Notification.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 8.

## Project definition (authoritative build metadata)

Source: `Algocura.Notification.Api.Tests/Algocura.Notification.Api.Tests.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
	<PackageReference Include="SQLitePCLRaw.lib.e_sqlite3" Version="2.1.12" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Algocura.Notification.Api\Algocura.Notification.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Notification.Api.Tests/Algocura.Notification.Api.Tests.csproj` | 29 |
| `Algocura.Notification.Api.Tests/Controllers/NotificationRulesControllerTests.cs` | 42 |
| `Algocura.Notification.Api.Tests/Controllers/NotificationSynchronizationControllerTests.cs` | 71 |
| `Algocura.Notification.Api.Tests/Controllers/NotificationsControllerTests.cs` | 85 |
| `Algocura.Notification.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Notification.Api.Tests/Messaging/PlatformEventsConsumerTests.cs` | 96 |
| `Algocura.Notification.Api.Tests/Services/NotificationComposerTests.cs` | 43 |
| `Algocura.Notification.Api.Tests/Support/TestDatabase.cs` | 41 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Notification.Api.Tests/Controllers/NotificationRulesControllerTests.cs`

- L13: `public async Task Create_ValidRequest_CreatesRule()`

### `Algocura.Notification.Api.Tests/Controllers/NotificationsControllerTests.cs`

- L16: `public async Task Dispatch_PendingNotification_MarksDelivered()`

### `Algocura.Notification.Api.Tests/Controllers/NotificationSynchronizationControllerTests.cs`

- L16: `public async Task SynchronizeCompletedNotifications_PublishesCompletedOnly()`
- L54: `private static Algocura.Notification.Api.Domain.Notification Create(NotificationStatus status) =>`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Notification.Api.Tests/Messaging/PlatformEventsConsumerTests.cs`
- L13: `public async Task Handle_MatchingRule_CreatesNotification()`
- L47: `public async Task Handle_Replay_DoesNotCreateDuplicate()`
- L81: `private static NotificationRule CreateRule() =>`

### `Algocura.Notification.Api.Tests/Services/NotificationComposerTests.cs`
- L10: `public void Compose_ReplacesTokensAndCreatesDelivery()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
