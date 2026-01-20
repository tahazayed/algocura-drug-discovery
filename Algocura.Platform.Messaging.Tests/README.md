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
