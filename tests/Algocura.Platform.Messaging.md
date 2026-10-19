# Existing test behavior index: Algocura.Platform.Messaging

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.Platform.Messaging.Tests/Consumers/ServiceBusConsumerBaseTests.cs

- L12: `[Fact]`
- L13: `public void Constructor_WithoutSubscriptionName_Throws()`
- L46: `[Fact]`
- L47: `public void ResolveEventType_UsesApplicationPropertyFirst()`
- L73: `[Fact]`
- L74: `public void ResolveEventType_FallsBackToSubject()`
- L93: `[Fact]`
- L94: `public void ResolveSource_UsesSourceApplicationProperty()`
- L117: `[Fact]`
- L118: `public void ResolveSource_WithoutSource_ReturnsUnknown()`
- L134: `[Fact]`
- L135: `public void ResolveEventId_WithGuidMessageId_ReturnsMessageId()`
- L154: `[Fact]`
- L155: `public void ResolveEventId_WithInvalidMessageId_GeneratesGuid()`
## Algocura.Platform.Messaging.Tests/DependencyInjection/MessagingServiceCollectionExtensionsTests.cs

- L14: `[Fact]`
- L15: `public void AddAlgocuraPlatformMessaging_RegistersSharedServices()`
- L65: `[Fact]`
- L66: `public void AddAlgocuraPlatformMessaging_BindsServiceBusOptions()`
## Algocura.Platform.Messaging.Tests/Events/IntegrationEventTests.cs

- L8: `[Fact]`
- L9: `public void IntegrationEvent_UsesVersionOneByDefault()`
- L21: `[Fact]`
- L22: `public void IntegrationEvent_ExposesRequiredMetadata()`
- L63: `[Fact]`
- L64: `public void IntegrationEvent_AllowsEventVersionOverride()`
## Algocura.Platform.Messaging.Tests/GlobalUsings.cs

## Algocura.Platform.Messaging.Tests/Options/ServiceBusOptionsTests.cs

- L8: `[Fact]`
- L9: `public void Defaults_ArePlatformDefaults()`
- L51: `[Fact]`
- L52: `public void SectionName_IsServiceBus()`
## Algocura.Platform.Messaging.Tests/Publishing/ServiceBusEventPublisherTests.cs

- L11: `[Fact]`
- L12: `public async Task PublishAsync_ForwardsEventAndStandardMetadata()`
- L74: `[Fact]`
- L75: `public async Task PublishAsync_WithNullEvent_Throws()`
