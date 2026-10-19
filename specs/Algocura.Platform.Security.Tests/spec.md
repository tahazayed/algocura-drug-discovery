# Algocura.Platform.Security.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 10.

## Original documented intent

# Algocura.Platform.Security.Tests

Unit tests for `Algocura.Platform.Security`.

## Coverage

```text
PlatformRoles
PlatformPolicies
PlatformClaimTypes
JwtOptions defaults
ClaimsPrincipal extensions
HttpCurrentUserContext
Dependency injection
JWT configuration binding
Default JWT bearer scheme
Built-in authorization policies
Role policy requirements
```

## Run

```powershell
dotnet test .\Algocura.Platform.Security.Tests\Algocura.Platform.Security.Tests.csproj
```

## Add to the solution

```powershell
dotnet sln add .\Algocura.Platform.Security.Tests\Algocura.Platform.Security.Tests.csproj
```

The tests use xUnit and FluentAssertions and do not require the Identity database
or a live identity provider.


## Project definition (authoritative build metadata)

Source: `Algocura.Platform.Security.Tests/Algocura.Platform.Security.Tests.csproj`
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
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Algocura.Platform.Security\Algocura.Platform.Security.csproj" />
  </ItemGroup>

</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Platform.Security.Tests/Algocura.Platform.Security.Tests.csproj` | 26 |
| `Algocura.Platform.Security.Tests/Authorization/PlatformPoliciesTests.cs` | 17 |
| `Algocura.Platform.Security.Tests/Authorization/PlatformRolesTests.cs` | 28 |
| `Algocura.Platform.Security.Tests/Claims/PlatformClaimTypesTests.cs` | 16 |
| `Algocura.Platform.Security.Tests/Context/HttpCurrentUserContextTests.cs` | 57 |
| `Algocura.Platform.Security.Tests/DependencyInjection/SecurityServiceCollectionExtensionsTests.cs` | 127 |
| `Algocura.Platform.Security.Tests/Extensions/ClaimsPrincipalExtensionsTests.cs` | 89 |
| `Algocura.Platform.Security.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Platform.Security.Tests/Options/JwtOptionsTests.cs` | 20 |
| `Algocura.Platform.Security.Tests/README.md` | 34 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

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
