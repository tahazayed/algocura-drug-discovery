# Algocura.Platform.Messaging.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 8.

## Original documented intent

# Algocura.Platform.Messaging.Tests

Unit tests for the shared `Algocura.Platform.Messaging` project.

## Coverage

The test project covers:

```text
IntegrationEvent / IIntegrationEvent
ServiceBusOptions defaults
ServiceBusEventPublisher
ServiceBusConsumerBase metadata helpers
ServiceBusConsumerBase subscription validation
AddAlgocuraPlatformMessaging registration
ServiceBusOptions configuration binding
```

The tests intentionally do not connect to a live Azure Service Bus namespace.

Azure Service Bus received-message instances are created with
`ServiceBusModelFactory`, which is provided by the Azure SDK specifically for
testing and mocking scenarios.

## Run

From the repository root:

```powershell
dotnet test .\Algocura.Platform.Messaging.Tests\Algocura.Platform.Messaging.Tests.csproj
```

Or from the test project directory:

```powershell
dotnet test
```

## Add to the solution

For a `.slnx` solution:

```powershell
dotnet sln add .\Algocura.Platform.Messaging.Tests\Algocura.Platform.Messaging.Tests.csproj
```

## Project dependency

The test project references:

```text
Algocura.Platform.Messaging
```

through:

```xml
<ProjectReference Include="..\Algocura.Platform.Messaging\Algocura.Platform.Messaging.csproj" />
```

The test suite uses:

```text
xUnit
FluentAssertions
NSubstitute
Microsoft.NET.Test.Sdk
```

## Test strategy

`ServiceBusEventPublisher` is tested against a substituted
`IPlatformMessagePublisher`, so publishing tests verify message metadata
without using Azure.

`ServiceBusConsumerBase` helper methods are tested using
`ServiceBusModelFactory.ServiceBusReceivedMessage(...)`.

The dependency-injection tests inspect registrations and configuration
binding without sending or receiving Service Bus messages.

Live Service Bus or emulator tests should be maintained separately as
integration tests rather than unit tests.


## Project definition (authoritative build metadata)

Source: `Algocura.Platform.Messaging.Tests/Algocura.Platform.Messaging.Tests.csproj`
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
    <ProjectReference Include="..\Algocura.Platform.Messaging\Algocura.Platform.Messaging.csproj" />
  </ItemGroup>

</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Platform.Messaging.Tests/Algocura.Platform.Messaging.Tests.csproj` | 26 |
| `Algocura.Platform.Messaging.Tests/Consumers/ServiceBusConsumerBaseTests.cs` | 234 |
| `Algocura.Platform.Messaging.Tests/DependencyInjection/MessagingServiceCollectionExtensionsTests.cs` | 155 |
| `Algocura.Platform.Messaging.Tests/Events/IntegrationEventTests.cs` | 106 |
| `Algocura.Platform.Messaging.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Platform.Messaging.Tests/Options/ServiceBusOptionsTests.cs` | 58 |
| `Algocura.Platform.Messaging.Tests/Publishing/ServiceBusEventPublisherTests.cs` | 123 |
| `Algocura.Platform.Messaging.Tests/README.md` | 83 |

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
