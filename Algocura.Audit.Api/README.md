# Algocura Audit API 

The Audit API is the centralized, read-only audit-query service for the
Algocura Drug Discovery Platform.

It consumes `EntityAuditEvent` messages emitted by platform services and
stores them in its own SQL Server database for traceability, investigation,
compliance support, and operational review.

## Responsibilities

- Consume `EntityAuditEvent`
- Persist immutable audit history
- Deduplicate events by `EventId`
- Search by service, entity, action, user, correlation, workflow, and date
- Retrieve a single audit event
- Produce aggregate audit summaries
- Expose audit synchronization metadata
- Preserve previous/current/changed-value JSON
- Preserve actor, trace, correlation, IP, and user-agent metadata
- Seed prototype audit records
- Use Algocura shared auditing for local persistence stamping without
  publishing audit events about the audit records themselves

## Endpoints

```text
GET /api/v1/audit
GET /api/v1/audit/{eventId}
GET /api/v1/audit/summary
GET /api/v1/audit/synchronization

GET /health
GET /swagger
GET /redoc
GET /openapi/v1.json
```

## Query example

```text
GET /api/v1/audit?
    serviceName=Algocura.Screening.Api&
    entityType=ScreeningJob&
    action=Completed&
    workflowId=71000000-0000-0000-0000-000000000001
```

## EntityAuditEvent

The Audit API wire contract follows the shared
`Algocura.Platform.Auditing.Events.EntityAuditEvent` schema:

```text
EventId
OccurredAtUtc
ServiceName
EntityType
EntityId
Action
UserId
Subject
UserDisplayName
UserEmail
ActorType
PreviousValuesJson
CurrentValuesJson
ChangedPropertiesJson
CorrelationId
WorkflowId
TraceId
IpAddress
UserAgent
EventType
EventVersion
```

## Audit feedback-loop protection

The Audit API registers:

```csharp
options.PublishEntityAuditEvents = false;
options.IgnoreAuditInfrastructureEntities = true;
```

Therefore its own `AuditRecord` inserts do not create another
`EntityAuditEvent`.

## Seed data

The seeder inserts deterministic prototype audit events covering:

```text
Literature Intelligence
Target Discovery
Chemical Design
Compound Catalog
Screening
Assay Management
AI Discovery
Workflow
Notification
Reporting
```

All seed persistence metadata uses:

```text
00000000-0000-0000-0000-000000000001
```

## Database migration

```powershell
Add-Migration InitialCreate `
    -Context AuditDbContext `
    -OutputDir Data/Migrations

Update-Database `
    -Context AuditDbContext
```

## Service Bus

Add the following subscription to `algocura-events`:

```text
audit-api
```

The subscription should receive `EntityAuditEvent` messages.

## Important platform integration

The Audit API can only receive entity-audit messages after the producing
services use a real `IAuditEventPublisher`.

Those services currently need to change from:

```csharp
builder.Services.AddAlgocuraAuditing(options =>
{
    options.PublishEntityAuditEvents = false;
});
```

to the generic registration that supplies a Service Bus implementation:

```csharp
builder.Services.AddAlgocuraAuditing<ServiceBusAuditEventPublisher>(
    options =>
    {
        options.ServiceName = "Algocura.Screening.Api";
        options.StampCreatedAndUpdatedFields = true;
        options.PublishEntityAuditEvents = true;
    });
```

The publisher should publish `EntityAuditEvent` to `algocura-events`
with:

```text
Subject = EntityAuditEvent
ApplicationProperties["eventType"] = EntityAuditEvent
```

A shared publisher implementation should preferably live in
`Algocura.Platform.Auditing` or another shared platform messaging library
so every microservice uses the same audit-event publishing contract.
