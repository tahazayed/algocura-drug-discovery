# Algocura.Search.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.Search.Api.Tests/Algocura.Search.Api.Tests.csproj`
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
    <ProjectReference Include="..\Algocura.Search.Api\Algocura.Search.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Search.Api.Tests/Algocura.Search.Api.Tests.csproj` | 29 |
| `Algocura.Search.Api.Tests/Controllers/SearchSynchronizationControllerTests.cs` | 77 |
| `Algocura.Search.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Search.Api.Tests/Messaging/PlatformSearchEventsConsumerTests.cs` | 66 |
| `Algocura.Search.Api.Tests/Services/SearchDocumentFactoryTests.cs` | 56 |
| `Algocura.Search.Api.Tests/Services/SearchServiceTests.cs` | 74 |
| `Algocura.Search.Api.Tests/Support/TestDatabase.cs` | 49 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Search.Api.Tests/Controllers/SearchSynchronizationControllerTests.cs`

- L16: `public async Task PublishIndexSnapshot_PublishesCounts()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Search.Api.Tests/Messaging/PlatformSearchEventsConsumerTests.cs`
- L16: `public async Task Handle_SupportedEvent_CreatesDocument()`

### `Algocura.Search.Api.Tests/Services/SearchDocumentFactoryTests.cs`
- L10: `public void Create_CompoundSnapshot_MapsDocument()`
- L41: `public void Create_UnsupportedEvent_ReturnsNull()`

### `Algocura.Search.Api.Tests/Services/SearchServiceTests.cs`
- L12: `public async Task Search_ByCompoundName_ReturnsMatch()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
