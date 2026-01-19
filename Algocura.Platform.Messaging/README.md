# Algocura.Platform.Messaging

Shared Azure Service Bus infrastructure for Algocura services.

## Configuration

```json
"ServiceBus": {
  "UseEmulator": true,
  "ConnectionString": "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
  "FullyQualifiedNamespace": "",
  "TopicName": "algocura-events"
}
```

## Register messaging

```csharp
using Algocura.Platform.Messaging.DependencyInjection;

builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);
```

## Register auditing with Service Bus publishing

```csharp
using Algocura.Platform.Auditing.DependencyInjection;
using Algocura.Platform.Auditing.Options;
using Algocura.Platform.Messaging.Publishing;

var auditingSection =
    builder.Configuration.GetSection(
        AuditingOptions.SectionName);

builder.Services
    .AddAlgocuraAuditing<ServiceBusAuditEventPublisher>(
        options =>
        {
            auditingSection.Bind(options);
        });
```

Set:

```json
"Auditing": {
  "ServiceName": "Algocura.CompoundCatalog.Api",
  "PublishEntityAuditEvents": true,
  "StampCreatedAndUpdatedFields": true,
  "IncludeUnchangedPropertiesOnCreate": true,
  "IncludeUnchangedPropertiesOnDelete": true,
  "IgnoreAuditInfrastructureEntities": true
}
```

The `EntityAuditEventInterceptor` will then resolve
`ServiceBusAuditEventPublisher` instead of `NullAuditEventPublisher`.
