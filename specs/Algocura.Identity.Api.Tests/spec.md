# Algocura.Identity.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 5.

## Project definition (authoritative build metadata)

Source: `Algocura.Identity.Api.Tests/Algocura.Identity.Api.Tests.csproj`
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
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Algocura.Identity.Api\Algocura.Identity.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Identity.Api.Tests/Algocura.Identity.Api.Tests.csproj` | 24 |
| `Algocura.Identity.Api.Tests/Data/IdentitySeederTests.cs` | 59 |
| `Algocura.Identity.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Identity.Api.Tests/Security/JwtTokenServiceTests.cs` | 102 |
| `Algocura.Identity.Api.Tests/Support/TestIdentityFactory.cs` | 49 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Identity.Api.Tests/Data/IdentitySeederTests.cs`
Types: `IdentitySeederTests`
- L1: `using Algocura.Identity.Api.Data;`
- L2: `using Algocura.Identity.Api.Domain;`
- L3: `using Algocura.Identity.Api.Tests.Support;`
- L9: `namespace Algocura.Identity.Api.Tests.Data;`

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
