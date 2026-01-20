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

