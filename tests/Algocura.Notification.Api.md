# Existing test behavior index: Algocura.Notification.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.Notification.Api.Tests/Controllers/NotificationRulesControllerTests.cs

- L12: `[Fact]`
- L13: `public async Task Create_ValidRequest_CreatesRule()`
## Algocura.Notification.Api.Tests/Controllers/NotificationSynchronizationControllerTests.cs

- L15: `[Fact]`
- L16: `public async Task SynchronizeCompletedNotifications_PublishesCompletedOnly()`
## Algocura.Notification.Api.Tests/Controllers/NotificationsControllerTests.cs

- L15: `[Fact]`
- L16: `public async Task Dispatch_PendingNotification_MarksDelivered()`
## Algocura.Notification.Api.Tests/GlobalUsings.cs

## Algocura.Notification.Api.Tests/Messaging/PlatformEventsConsumerTests.cs

- L12: `[Fact]`
- L13: `public async Task Handle_MatchingRule_CreatesNotification()`
- L46: `[Fact]`
- L47: `public async Task Handle_Replay_DoesNotCreateDuplicate()`
## Algocura.Notification.Api.Tests/Services/NotificationComposerTests.cs

- L9: `[Fact]`
- L10: `public void Compose_ReplacesTokensAndCreatesDelivery()`
## Algocura.Notification.Api.Tests/Support/TestDatabase.cs

