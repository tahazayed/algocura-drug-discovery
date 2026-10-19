# Algocura.Workflow.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 6.

## Project definition (authoritative build metadata)

Source: `Algocura.Workflow.Api.Tests/Algocura.Workflow.Api.Tests.csproj`
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
    <ProjectReference Include="..\Algocura.Workflow.Api\Algocura.Workflow.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Workflow.Api.Tests/Algocura.Workflow.Api.Tests.csproj` | 31 |
| `Algocura.Workflow.Api.Tests/Controllers/WorkflowsControllerTests.cs` | 65 |
| `Algocura.Workflow.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Workflow.Api.Tests/Messaging/PlatformWorkflowEventsConsumerTests.cs` | 166 |
| `Algocura.Workflow.Api.Tests/Services/WorkflowStateMachineTests.cs` | 106 |
| `Algocura.Workflow.Api.Tests/Support/TestDatabase.cs` | 49 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Workflow.Api.Tests/Controllers/WorkflowsControllerTests.cs`

- L17: `public async Task Create_ValidRequest_CreatesSevenStages()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Workflow.Api.Tests/Messaging/PlatformWorkflowEventsConsumerTests.cs`
- L13: `public async Task Handle_Event_CompletesStageAndStoresReceipt()`
- L70: `public async Task Handle_ReplayedEvent_IsIdempotent()`
- L121: `private static DiscoveryWorkflow CreateWorkflow()`

### `Algocura.Workflow.Api.Tests/Services/WorkflowStateMachineTests.cs`
- L10: `public void Start_DraftWorkflow_ActivatesLiteratureStage()`
- L39: `public void CompleteStage_Literature_ActivatesTargetDiscovery()`
- L70: `private static DiscoveryWorkflow CreateWorkflow()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
