# Algocura.Platform.Auditing

Shared auditing library for Algocura services.

## Included components

- `IAuditableEntity`
- `IOwnedEntity`
- `IApprovedEntity`
- `ICompletedEntity`
- `IProjectionAuditMetadata`
- `ICurrentUserContext`
- `CurrentUserContext`
- `AuditableEntityInterceptor`
- `EntityAuditEventInterceptor`
- `EntityAuditEvent`
- `IAuditEventPublisher`
- `IAuditValueSanitizer`
- `DefaultAuditValueSanitizer`
- Dependency-injection extensions

## Register the library

```csharp
builder.Services.AddAlgocuraAuditing(options =>
{
    options.ServiceName =
        "Algocura.CompoundCatalog.Api";

    options.PublishEntityAuditEvents =
        true;
});
```

For a custom publisher:

```csharp
builder.Services.AddAlgocuraAuditing<
    ServiceBusAuditEventPublisher>(
    options =>
    {
        options.ServiceName =
            "Algocura.CompoundCatalog.Api";
    });
```

## Add the interceptors to a DbContext

```csharp
builder.Services.AddDbContext<CompoundCatalogDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            databaseConnectionString);

        options.AddInterceptors(
            serviceProvider.GetRequiredService<
                AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<
                EntityAuditEventInterceptor>());
    });
```

## Implement the auditable interface

```csharp
public sealed class Compound : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
```

## Identity source

`CurrentUserContext` reads the current authenticated identity from:

- `sub`
- `ClaimTypes.NameIdentifier`
- `oid`

It also reads:

- display name
- email
- correlation ID
- trace ID
- IP address
- user agent

Do not accept audit user IDs from request bodies.

## Sensitive properties

The default sanitizer excludes:

- passwords
- access tokens
- refresh tokens
- client secrets
- connection strings
- shared access keys
- API keys

It masks:

- email
- recipient
- IP address
- user agent

Extend these collections through `AuditingOptions`.

## Important transaction note

`EntityAuditEventInterceptor` publishes after the database save succeeds.

This avoids publishing audit events for failed writes, but it does not
provide atomic database-and-message consistency. For production-grade
guarantees, implement the transactional outbox pattern in each service
and publish audit events from the outbox.
