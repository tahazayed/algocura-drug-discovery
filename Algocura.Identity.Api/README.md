# Algocura Identity API

The Identity API provides authentication, user management, role management,
and JWT bearer tokens for the Algocura Drug Discovery Platform.

## Responsibilities

- Register users
- Authenticate users
- Issue JWT access tokens
- Return the authenticated user's profile
- List users
- Retrieve users
- Assign roles
- Disable users
- Seed platform roles
- Seed the fixed system identity
- Optionally seed a development administrator
- Integrate with `Algocura.Platform.Auditing`
- Publish entity audit events through `Algocura.Platform.Messaging`
- Persist identity data in a dedicated SQL Server database


## Shared platform messaging

The Identity API uses `Algocura.Platform.Messaging` for Azure Service Bus
transport. Identity entity changes are audited by `Algocura.Platform.Auditing`
and published to the shared `algocura-events` topic.

```text
Identity entity change
        ↓
EntityAuditEventInterceptor
        ↓
ServiceBusAuditEventPublisher
        ↓
IPlatformMessagePublisher
        ↓
ServiceBusMessagePublisher
        ↓
Azure Service Bus
        ↓
Topic: algocura-events
        ↓
Audit API
```

Registration is centralized:

```csharp
builder.Services.AddAlgocuraPlatformMessaging(
    builder.Configuration);

builder.Services
    .AddAlgocuraAuditing<
        ServiceBusAuditEventPublisher>(
        options =>
        {
            builder.Configuration
                .GetSection(
                    AuditingOptions.SectionName)
                .Bind(options);
        });
```

The Identity API does not require its own Service Bus subscription because it
publishes audit events but does not currently consume platform events.

The shared messaging project owns `ServiceBusClient`, `ServiceBusOptions`,
`IPlatformMessagePublisher`, `ServiceBusMessagePublisher`,
`IEventPublisher`, `ServiceBusEventPublisher`, and
`ServiceBusAuditEventPublisher`.

## Roles

The seed process creates:

```text
Administrator
Scientist
Reviewer
Service
```

## System identity

The fixed system identity is:

```text
00000000-0000-0000-0000-000000000001
```

It is stored as:

```text
system@algocura.local
```

and assigned the `Service` role.

## Development administrator

An administrator is only seeded when this configuration value is supplied:

```text
Seed:AdministratorPassword
```

The seeded administrator is:

```text
admin@algocura.local
```

Its fixed ID is:

```text
c1000000-0000-0000-0000-000000000001
```

Do not store a production administrator password in source-controlled
configuration. Supply it using environment variables, a secret store,
or another approved secret-management mechanism.

## Endpoints

```text
POST /api/v1/auth/register
POST /api/v1/auth/login
GET  /api/v1/auth/me

GET  /api/v1/users
GET  /api/v1/users/{id}
POST /api/v1/users/{id}/roles
POST /api/v1/users/{id}/disable

GET /health
GET /swagger
GET /redoc
GET /openapi/v1.json
```

The `/api/v1/users` endpoints require the `Administrator` role.

## Registration

Example request:

```json
{
  "email": "scientist@algocura.local",
  "password": "Example!Pass123",
  "displayName": "Algocura Scientist"
}
```

Newly registered users are assigned the `Scientist` role.

## Login

```json
{
  "email": "scientist@algocura.local",
  "password": "Example!Pass123"
}
```

The response contains a JWT access token and user profile.

## JWT claims

Tokens include:

```text
sub
email
ClaimTypes.NameIdentifier
ClaimTypes.Name
ClaimTypes.Role
```

## Password policy

```text
Minimum length: 12
Digit required
Uppercase required
Lowercase required
Non-alphanumeric required
Maximum failed attempts: 5
Lockout: 15 minutes
```

## Database migration

```powershell
Add-Migration InitialCreate `
    -Context IdentityDbContext `
    -OutputDir Data/Migrations

Update-Database `
    -Context IdentityDbContext
```

## Docker

Example environment configuration:

```yaml
identity-api:
  image: algocura/identity-api:dev
  build:
    context: .
    dockerfile: Algocura.Identity.Api/Dockerfile
  ports:
    - "7013:8080"
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ConnectionStrings__IdentityDatabase: >
      Server=sqlserver,1433;
      Database=AlgocuraIdentity;
      User Id=sa;
      Password=DontLogin!Passw0rd;
      TrustServerCertificate=True
    Jwt__Issuer: Algocura.Identity.Api
    Jwt__Audience: Algocura.Platform
    Jwt__SigningKey: ${ALGOCURA_JWT_SIGNING_KEY}
    Seed__AdministratorPassword: ${ALGOCURA_ADMIN_PASSWORD}
```

## Production direction

The current Identity API is a platform prototype that owns local ASP.NET
Core Identity users and issues symmetric-key JWTs.

For an enterprise deployment, authentication can later be federated with
Microsoft Entra ID while preserving Algocura application roles and user IDs
where appropriate.
